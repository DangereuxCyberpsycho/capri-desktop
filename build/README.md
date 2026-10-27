# Compiler Capri

Deux projets, pour .NET Framework 4.8 (déjà présent sur Windows 10 et 11) :

- `Capri.App.csproj` : l'appli, `Capri.exe`
- `Capri.Setup.csproj` : l'installateur, `Capri-Setup.exe`, qui embarque `Capri.exe`

## Le plus simple : laisser GitHub compiler

Chaque push sur `main` ou `dev` lance le workflow **Windows** (onglet *Actions* du dépôt). Il compile les deux programmes, lance les auto-tests de Capri et publie un artefact **Capri-windows** contenant `Capri.exe` et `Capri-Setup.exe`.

## Compiler sur ton PC

1. Installe le [SDK .NET](https://dotnet.microsoft.com/download) (version 8 ou plus récente).
2. Dans le dossier du dépôt :

```
dotnet build build/Capri.App.csproj -c Release -o build/out
dotnet build build/Capri.Setup.csproj -c Release -o build/setup-out
```

3. Lance `build/out/Capri.exe`, ou installe avec `build/setup-out/Capri-Setup.exe`.

Auto-tests : `build/out/Capri.exe --smoke-test smoke`, puis regarde `smoke/smoke-test.txt` (une ligne `FAIL` = un test raté, `smoke/failure.txt` = un plantage).
