# ADR 0001: Migration vers MAUI et Consolidation Architecture

**Date**: 2025-11-21  
**Status**: Accepté  
**Décideurs**: Équipe ADHD Workspace

## Contexte

Le projet ADHDWorkspace était initialement composé de multiples scripts C# indépendants (WinForms) dans le dossier `scripts/`, chacun compilé en .exe séparé. Cette approche fragmentée créait des problèmes de maintenance, duplication de code, et complexité d'utilisation.

### Problèmes identifiés:
- **Duplication massive**: Code identique dans `ADHDWorkspace.cs`, `MiniTaskbar.cs`, `DevPomodoro.cs`, etc.
- **Exécution fragmentée**: Chaque fonctionnalité nécessite un .exe différent
- **Pas de partage d'état**: Communication via fichiers texte dans `C:\Temp\`
- **Absence de configuration centralisée**: Chemins et paramètres codés en dur
- **Expérience utilisateur complexe**: Nécessite de lancer plusieurs programmes
- **Pas de logging unifié**: Difficile de diagnostiquer les problèmes
- **Architecture WinForms obsolète**: Ne permet pas l'évolution cross-platform

## Décision

Nous avons décidé de **migrer vers .NET MAUI 10** et de **consolider toutes les fonctionnalités** dans une application unifiée avec architecture services.

### Architecture retenue:

```
ADHDWorkspace/
├── src/
│   ├── Program.cs              # Entry point avec Mutex + CLI routing
│   ├── MauiProgram.cs          # DI configuration MAUI
│   ├── App.xaml                # Application MAUI (dark theme)
│   ├── Commands/               # Commandes CLI
│   │   ├── WindowCommands.cs
│   │   ├── OtherCommands.cs
│   │   └── AutoStartCommand    # NEW: Registry auto-start
│   ├── Services/               # Services métier + infrastructure
│   │   ├── IConfigurationService.cs / ConfigurationService.cs
│   │   ├── LoggerService.cs    # Serilog avec rotation
│   │   ├── INotificationService.cs / NotificationService.cs
│   │   ├── IScreenService.cs / ScreenService.cs
│   │   ├── IHotkeyService.cs / WindowsHotkeyService.cs
│   │   ├── PomodoroService.cs
│   │   ├── FocusTrackerService.cs
│   │   ├── ObsidianOverlayService.cs
│   │   ├── ContextService.cs
│   │   └── WorkspaceSetupService.cs
│   ├── Views/                  # Pages MAUI
│   │   ├── MiniTaskbarWindow.xaml/cs    # Unified taskbar
│   │   ├── SettingsPage.xaml/cs         # Configuration UI
│   │   ├── QuickCapturePage.xaml/cs
│   │   └── RestoreContextPage.xaml/cs
│   └── Models/
│       ├── AppConfig.cs        # AppSettings classes
│       └── Models.cs
├── config/
│   └── appsettings.json        # Configuration JSON
├── tests/                       # NEW: Tests xUnit
│   ├── Commands/
│   └── Services/
└── docs/adr/                    # NEW: Documentation ADR
```

## Choix techniques clés

### 1. **Migration .NET MAUI 10**
- **Rationnel**: Cross-platform future (Windows 11+, macOS possible)
- **Bénéfices**: UI moderne, animations natives, meilleure intégration système
- **Trade-off**: Complexité initiale > WinForms, mais meilleur long-terme

### 2. **Services en arrière-plan avec injection de dépendances**
- **Rationnel**: Séparation concerns, testabilité, réutilisabilité
- **Pattern**: Constructor injection via `MauiProgram.CreateMauiApp()`
- **Lifecycle**: Singletons pour services, Transient pour vues

### 3. **Configuration JSON + UI Settings**
- **Fichier**: `config/appsettings.json` avec structure typée (`AppSettings`)
- **Backup automatique**: `.json.backup` avant chaque sauvegarde
- **UI intégrée**: `SettingsPage` avec validation temps réel et dirty tracking
- **Mode portable**: `--portable` utilise chemins locaux au lieu système

### 4. **Logging avec Serilog**
- **Sinks**: Console + Fichier rotatif (7 jours)
- **Path**: `C:\Temp\adhd-workspace-.log` (ou `data/` en portable)
- **Niveau**: Configurable via appsettings + override `--verbose`
- **Format**: Timestamps, contexte, stack traces

### 5. **Notifications Windows Toast**
- **Library**: `Microsoft.Toolkit.Uwp.Notifications`
- **Usage**: Transitions Pomodoro, erreurs hotkeys, confirmations
- **Types**: Info (ShowToast), Erreur (ShowError), Succès (ShowSuccess)

### 6. **Hotkeys système avec validation**
- **Interface**: `IHotkeyService` avec implémentation `WindowsHotkeyService`
- **Platform-specific**: `#if WINDOWS` pour RegisterHotKey P/Invoke
- **Validation**: Toast d'erreur si conflit ou échec enregistrement
- **Configuration**: `appsettings.json` section Hotkeys (format "Win+Shift+N")

### 7. **MiniTaskbar tout-en-un**
- **Mode défaut**: Lance tous les services (Pomodoro, Focus, Overlay)
- **UI**:
  - **Collapsed**: 275x75px, horloge + indicateurs (●/🎯/⚙️/×) + progressbar
  - **Expanded**: 400x450px, + 8 boutons apps + focus task label
  - **Animations**: Fade in/out, resize smooth (CommunityToolkit.Maui)
- **Positionnement**: Haut-droite écran rightmost via `ScreenService`
- **Focus indicator**: 🎯 (idle bleu) → 🔴 (actif rouge) selon `focus-tracker-state.txt`
- **Tray icon**: Planned (NotifyIcon via CommunityToolkit.Maui)

### 8. **Mutex instance unique**
- **Name**: "ADHDWorkspace_SingleInstance"
- **Scope**: GUI mode uniquement (pas CLI commands)
- **Behavior**: Toast "Already Running" si instance existante

### 9. **Mode CLI préservé**
- **Commandes**: `setup`, `capture`, `restore`, `save [name]`, `autostart [enable|disable|status]`, `settings`
- **Arguments**: `--verbose` (debug logging), `--portable` (local storage)
- **Défaut**: Sans argument, lance `MiniTaskbarWindow`

### 10. **Tests xUnit ciblés**
- **Scope**: Commands critiques + Services infrastructure
- **Tools**: xUnit + Moq pour mocking
- **Coverage**: Pas de target strict, focus cas critiques (Registry, Config save/load, portable mode)

## Conséquences

### Positives ✅
1. **Une seule commande**: `ADHDWorkspace.exe` lance tout
2. **Configuration centralisée**: Editable via UI ou JSON
3. **Services intégrés**: Pomodoro, Focus, Overlay communiquent via DI
4. **Logging unifié**: Tous logs dans un fichier, niveaux configurables
5. **Notifications cohérentes**: Toast standardisés pour feedback utilisateur
6. **Hotkeys validés**: Détection conflits en temps réel
7. **Mode portable**: Support USB/partage sans installation
8. **Tests automatisés**: Regression catching pour features critiques
9. **Maintenance facilitée**: Code mutualisé, pas de duplication
10. **Évolution future**: Base MAUI permet Android/macOS si besoin

### Négatives ⚠️
1. **Complexité initiale**: Migration MAUI > WinForms simple
2. **Dépendances lourdes**: Packages MAUI + CommunityToolkit + Serilog (~50MB)
3. **Windows 10.0.19041+**: Minimum OS requirement > ancien WinForms (Win7+)
4. **Hotkeys Windows-only**: Autres plateformes nécessiteront implémentation alternative
5. **Learning curve**: Équipe doit apprendre MAUI XAML

### Risques atténués 🛡️
- **Performance**: Services background légers (1s timers), impact minimal
- **Stabilité**: Serilog capture exceptions, Toast feedback utilisateur
- **Rétrocompatibilité**: État fichiers (`C:\Temp\*.txt`) préservé pour compatibilité scripts legacy temporaire

## Alternatives considérées

### ❌ Garder WinForms consolidé
- **Rejeté**: Pas d'animations, UI datée, pas de cross-platform
- **Avantage**: Simplicité, pas de migration
- **Inconvénient**: Limite évolution, UI moins moderne

### ❌ Electron/Web app
- **Rejeté**: Overhead mémoire énorme (~200MB), pas d'intégration système native
- **Avantage**: Cross-platform facile, dev web
- **Inconvénient**: RegisterHotKey difficile, pas de vraie intégration Windows

### ❌ Avalonia UI
- **Rejeté**: Moins de support Microsoft, communauté plus petite
- **Avantage**: Plus léger que MAUI, cross-platform
- **Inconvénient**: Moins de ressources, intégration MAUI Services complexe

## Implémentation

### Migration steps (complétées):
1. ✅ Conversion `.csproj` vers MAUI SDK avec packages
2. ✅ Création `appsettings.json` + classes `AppSettings`
3. ✅ Implémentation services infrastructure (Config, Logger, Notification, Screen, Hotkey)
4. ✅ Implémentation services métier (Pomodoro, FocusTracker, Obsidian, Context, WorkspaceSetup)
5. ✅ Configuration `MauiProgram.cs` avec DI
6. ✅ Migration vues vers MAUI XAML (MiniTaskbarWindow, SettingsPage, QuickCapture, RestoreContext)
7. ✅ Update `Program.cs` avec Mutex, args parsing, routing
8. ✅ Implémentation `AutoStartCommand` Registry
9. ✅ Création projet tests xUnit
10. ⏳ Suppression `scripts/` (à faire après validation complète)

### Rollback plan:
- Scripts originaux conservés temporairement dans `scripts/` jusqu'à validation complète
- Backup Git avant suppression: `git tag v1.0-legacy-scripts`
- Si problème majeur: revert commit, rebuild scripts avec `Compile-All.ps1`

## Validation

### Critères de succès:
- ✅ Compilation sans erreur en .NET 10
- ⏳ Tous tests xUnit passent (une fois exécutés)
- ⏳ MiniTaskbar démarre et affiche horloge
- ⏳ Pomodoro démarre et met à jour progressbar
- ⏳ Focus tracker détecte distractions
- ⏳ Hotkeys fonctionnent sans conflit
- ⏳ Settings sauvegarde et charge correctement
- ⏳ Mode portable utilise chemins locaux
- ⏳ AutoStart enable/disable fonctionne

## Références

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Serilog Best Practices](https://github.com/serilog/serilog/wiki/Configuration-Basics)
- [Windows Toast Notifications](https://learn.microsoft.com/en-us/windows/apps/design/shell/tiles-and-notifications/toast-notifications-overview)
- [Dependency Injection in MAUI](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection)

## Notes

Cette décision représente un refactoring majeur mais nécessaire pour la pérennité du projet. L'investissement initial en complexité est compensé par la maintenabilité long-terme et les capacités d'évolution.

Le choix MAUI sur Avalonia/WPF/Electron était principalement motivé par:
1. Support officiel Microsoft
2. Investissement futur (MAUI est la direction .NET)
3. Intégration native Windows excellente
4. Possibilité Android/macOS sans rewrite complet

---

**Prochaines étapes**:
1. Tester compilation complète
2. Exécuter tests xUnit
3. Validation manuelle fonctionnalités critiques
4. Suppression `scripts/` après 1 semaine de validation
5. Création installeur MSI/MSIX pour distribution
