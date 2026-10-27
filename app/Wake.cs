using System;
using System.Collections.Generic;
using System.Speech.Recognition;

namespace CapriDesktop
{
    // Listens on the default mic for "Hé Capri". Recognition runs offline on this PC; nothing is sent anywhere.
    internal sealed class WakeWord : IDisposable
    {
        internal const float Sure = 0.6f;
        private static readonly string[] French = { "hé capri", "eh capri", "et capri", "hey capri", "ok capri", "salut capri" };
        private static readonly string[] English = { "hey capri", "hi capri", "ok capri", "yo capri" };
        private readonly object gate = new object();
        private SpeechRecognitionEngine engine;
        private DateTime last = DateTime.MinValue;
        internal event Action<string> Called;

        internal bool Listening { get { lock (gate) return engine != null; } }

        internal void Start()
        {
            lock (gate)
            {
                if (engine != null) return;
                List<RecognizerInfo> installed = new List<RecognizerInfo>(SpeechRecognitionEngine.InstalledRecognizers());
                List<string> cultures = installed.ConvertAll(delegate(RecognizerInfo info) { return info.Culture.Name; });
                string chosen = Hearing.Pick(cultures, Hearing.Wanted());
                if (chosen == null) throw new InvalidOperationException(Hearing.NoRecognizer);
                RecognizerInfo picked = installed[cultures.IndexOf(chosen)];
                SpeechRecognitionEngine made = new SpeechRecognitionEngine(picked);
                try
                {
                    GrammarBuilder phrases = new GrammarBuilder(new Choices(picked.Culture.TwoLetterISOLanguageName == "fr" ? French : English)) { Culture = picked.Culture };
                    made.LoadGrammar(new Grammar(phrases) { Name = "wake" });
                    made.LoadGrammar(new DictationGrammar { Name = "other" });
                    made.SpeechRecognized += Recognized;
                    made.SetInputToDefaultAudioDevice();
                    made.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch (Exception) { made.Dispose(); throw; }
                engine = made;
            }
        }

        private void Recognized(object sender, SpeechRecognizedEventArgs e)
        {
            string request;
            if (e.Result.Grammar != null && e.Result.Grammar.Name == "wake") request = e.Result.Confidence >= Sure ? "" : null;
            else request = Hands.Called(e.Result.Text);
            if (request == null) return;
            lock (gate)
            {
                if (DateTime.UtcNow - last < TimeSpan.FromSeconds(3)) return;
                last = DateTime.UtcNow;
            }
            Action<string> called = Called;
            if (called != null) called(request);
        }

        internal void Stop()
        {
            SpeechRecognitionEngine old;
            lock (gate) { old = engine; engine = null; }
            if (old == null) return;
            old.SpeechRecognized -= Recognized;
            try { old.RecognizeAsyncCancel(); } catch (Exception) { }
            old.Dispose();
        }

        public void Dispose() { Stop(); }
    }
}
