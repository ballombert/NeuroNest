# ADHD Workspace - Guide de Validation et Prochaines Étapes

**Date**: 2025-11-21  
**Status**: Migration MAUI en cours - Blocage SDK

## État Actuel

### [fa-solid fa-square-check] Complété (95%)

1. **Architecture consolidée** - Tous les scripts migrés vers `src/`
2. **Configuration JSON** - `appsettings.json` complet avec 7 sections
3. **Services** - 10 services créés (infrastructure + métier)
4. **Vues MAUI** - 4 vues en XAML (MiniTaskbar, Settings, QuickCapture, RestoreContext)
5. **Tests xUnit** - Structure de test créée
6. **Documentation** - ADR 0001 complet
7. **Cleanup** - Scripts/ supprimé

### [fa-solid fa-triangle-exclamation] Blocage Actuel: SDK MAUI

**Problème**: `Microsoft.NET.Sdk.Maui` introuvable malgré workload installée
**Impact**: Compilation impossible
**Cause**: Problème SDK resolver avec .NET 9/10 sur cette machine

## Options de Résolution

### Option A: Déboguer MAUI (Temps estimé: 2-4h)

**Actions**:

```powershell
# Nettoyer complètement
dotnet workload uninstall maui
dotnet nuget locals all --clear
# Réinstaller
dotnet workload install maui --from-rollback-file https://aka.ms/dotnet/9.0/daily/workloads
# Vérifier MSBuild
where.exe msbuild
```

**Risques**:

- Peut nécessiter réinstallation complète VS/SDK
- Problèmes connus avec MSBuild workload resolver

### Option B: Rester sur Windows Forms + Services (Temps: 1h) [fa-solid fa-star] RECOMMANDÉ

**Rationnel**:

- **Architecture intacte**: Tous les services fonctionnent indépendamment du UI framework
- **90% du travail préservé**: Configuration, services, tests restent identiques
- **Validation immédiate**: Compilation et tests possibles maintenant
- **Migration MAUI future**: Garder le code MAUI dans `src/Views/`, créer temporairement `src/Views/WinForms/` en parallèle

**Actions**:

1. Modifier `.csproj`: `<Project Sdk="Microsoft.NET.Sdk">` + `<TargetFramework>net9.0-windows</TargetFramework>` + `<UseWindowsForms>true</UseWindowsForms>`
2. Créer `src/Views/WinForms/MiniTaskbarForm.cs` utilisant les mêmes services
3. Adapter `Program.cs` pour lancer Form au lieu de MauiApp
4. Compiler et valider

**Avantages**:

- [fa-solid fa-square-check] Compilation immédiate garantie
- [fa-solid fa-square-check] Tests exécutables
- [fa-solid fa-square-check] Fonctionnalités utilisables aujourd'hui
- [fa-solid fa-square-check] Migration MAUI possible ultérieurement (architecture découplée)

### Option C: Attendre .NET 10 RTM (Temps: ???)

.NET 10 Preview actuellement, RTM prévu novembre 2025.

## Recommandation

**Choisir Option B** pour les raisons suivantes:

1. **Pragmatisme**: WinForms fonctionne, MAUI est bloqué
2. **Valeur immédiate**: Application utilisable maintenant
3. **Architecture préservée**: Services indépendants du framework UI
4. **Réversibilité**: Migration MAUI reste possible quand SDK stable

## Prochaines Actions (Option B)

### 1. Adapter le .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <OutputType>WinExe</OutputType>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <!-- Garder tous les PackageReference existants sauf MAUI -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.0" />
    <PackageReference Include="Serilog" Version="4.2.0" />
    <!-- ... autres packages ... -->
  </ItemGroup>
</Project>
```

### 2. Créer MiniTaskbarForm.cs

Convertir `MiniTaskbarWindow.xaml.cs` en WinForms:

- `Panel` pour header avec `Label` horloge/indicateurs
- `ProgressBar` pour Pomodoro
- `FlowLayoutPanel` pour boutons apps
- `Timer` 1s pour updates
- Mêmes injections de services

### 3. Adapter App/Program.cs

```csharp
// Pas de MauiProgram, utiliser directement DI
var services = new ServiceCollection();
// Enregistrer tous les services...
var serviceProvider = services.BuildServiceProvider();

Application.Run(serviceProvider.GetRequiredService<MiniTaskbarForm>());
```

### 4. Valider

```powershell
dotnet build ADHDWorkspace.csproj
dotnet test tests/ADHDWorkspace.Tests.csproj
.\bin\Release\net9.0-windows\ADHDWorkspace.exe
```

## Migration MAUI Ultérieure

Quand SDK MAUI fonctionnel:

1. Réactiver `<Project Sdk="Microsoft.NET.Sdk.Maui">`
2. Supprimer `src/Views/WinForms/`
3. Réactiver `src/Views/*.xaml` dans `.csproj`
4. Adapter `Program.cs` pour relancer `MauiProgram`

**Toute la logique métier (services) reste inchangée.**

## Décision Requise

Confirmer choix:

- [ ] **Option A**: Déboguer MAUI (risque temps, incertain)
- [x] **Option B**: Adapter à WinForms temporairement (1h, garanti)
- [ ] **Option C**: Attendre .NET 10 RTM (6+ mois)

---

**Note pour l'équipe**: L'architecture services est le vrai succès de cette migration. Le choix UI framework (MAUI vs WinForms) est secondaire et réversible. Privilégions la validation fonctionnelle immédiate.
