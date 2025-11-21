# ADHD Workspace - Application MAUI .NET 9

> **Application Windows unifiée pour développeurs avec TDAH**  
> Workspace automatisé avec Pomodoro, Focus Tracker, Quick Capture et gestion de contexte

## [fa-solid fa-bullseye] Vue d'Ensemble

Application MAUI .NET 9 consolidée qui remplace tous les scripts PowerShell par une interface graphique moderne avec:

- [fa-solid fa-square-check] **MiniTaskbar** - Barre toujours visible avec horloge, Pomodoro, Focus Tracker
- [fa-solid fa-square-check] **Pomodoro 50/10/30** - Cycles focus automatiques avec Focus Assist
- [fa-solid fa-square-check] **Focus Tracker** - Détection distractions et rappels
- [fa-solid fa-square-check] **Quick Capture** - Capture rapide idées dans Inbox.md
- [fa-solid fa-square-check] **Context Save/Restore** - Sauvegarde automatique workspace VS Code
- [fa-solid fa-square-check] **Configuration UI** - Settings graphiques pour tous les paramètres
- [fa-solid fa-square-check] **Hotkeys globaux** - Raccourcis système configurables
- [fa-solid fa-square-check] **Mode portable** - Support clé USB sans installation

## [fa-solid fa-clipboard-list] Nouvelles Fonctionnalités

### Application Unifiée
- **Single EXE** - Plus besoin de lancer plusieurs scripts
- **Auto-start** - Commande `adhd autostart enable` pour démarrage Windows
- **Services intégrés** - Tous les services démarrent automatiquement
- **Toast notifications** - Feedback visuel pour toutes les actions

### Interface Moderne
- **Dark theme** - Interface sombre par défaut
- **Animations** - Transitions fluides (expand/collapse)
- **Progress bar** - Visualisation Pomodoro temps restant
- **Focus indicator** - Icône [fa-solid fa-bullseye] → [fa-solid fa-circle-dot] quand focus actif
- **DPI aware** - S'adapte à tous les écrans

## [fa-solid fa-rocket] Installation

### Prérequis

1. **Windows 10.0.19041+** (Windows 10 20H1 ou Windows 11)
2. **.NET 9 Runtime** (inclus avec l'application)
3. **Obsidian** (optionnel, pour overlay)
4. **VS Code** (optionnel, pour context save/restore)

### Installation Simple

1. **Télécharger** le dossier `bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\`
2. **Lancer** `ADHDWorkspace.exe`
3. **Configurer** via Settings ([fa-solid fa-gear]) dans le MiniTaskbar

### Configuration Initiale

Au premier lancement, l'application crée `config\appsettings.json` avec paramètres par défaut:

```json
{
  "Logging": {
    "MinimumLevel": "Information",
    "FilePath": "C:\\Temp\\adhd-workspace-.log"
  },
  "Paths": {
    "ProjectPath": "C:\\WORK\\Perso",
    "NotesPath": "C:\\WORK\\Perso\\adhd\\notes",
    "InboxPath": "C:\\WORK\\Perso\\adhd\\notes\\Inbox.md",
    "DailyNotesPath": "C:\\WORK\\Perso\\adhd\\notes\\Daily",
    "ContextHistoryPath": "C:\\WORK\\Perso\\adhd\\context-history",
    "ObsidianPath": "C:\\Users\\{User}\\AppData\\Local\\Obsidian\\Obsidian.exe",
    "VSCodePath": "code",
    "TerminalPath": "wt"
  },
  "Pomodoro": {
    "FocusMinutes": 50,
    "ShortBreakMinutes": 10,
    "LongBreakMinutes": 30,
    "CyclesBeforeLongBreak": 4
  },
  "FocusTracker": {
    "CheckIntervalSeconds": 15,
    "ReminderCooldownMinutes": 5,
    "DistractionApps": ["chrome", "firefox", "msedge", "slack", "discord"],
    "ProductiveApps": ["code", "devenv", "rider", "obsidian"]
  },
  "Hotkeys": {
    "QuickCapture": "Win+Shift+N",
    "StartPomodoro": "Win+Alt+F",
    "StartFocus": "Win+Alt+D",
    "SaveContext": "Win+Alt+S"
  },
  "UI": {
    "TaskbarCollapsedWidth": 275,
    "TaskbarCollapsedHeight": 75,
    "TaskbarExpandedWidth": 400,
    "TaskbarExpandedHeight": 450,
    "ObsidianOverlayWidth": 700,
    "ObsidianOverlayOpacityFocus": 50,
    "ObsidianOverlayOpacityBreak": 100,
    "ObsidianOverlayOpacityBackground": 80
  },
  "PortableMode": false
}
```

## [fa-solid fa-screwdriver-wrench] Build & Automatisation

### Regeneration Icône Système

L'icône système (tray icon) se génère automatiquement depuis `Resources/Images/Logo.png`:

```powershell
# Regenerate tray icon uniquement
.\build.ps1 --target GenerateTrayIcon

# Full build (inclut régénération icône)
.\build.ps1 --target Build
```

**Prérequis:**
- **Python 3.7+** (vérifier: `python --version`)
- **Pillow** (installer: `pip install pillow`)

**Processus:**
1. `build.ps1` installe Cake.Tool globalement si absent
2. Cake valide Python + Pillow disponibles
3. Script `scripts/convert_logo_to_tray_icon.py` convertit `Logo.png` → `trayicon.ico`
4. Génère multi-résolution (16x16, 32x32, 48x48, 256x256)
5. `TrayIconService` charge nouveau `.ico` au prochain lancement

**Menu Tray Icon:**
- **Start Pomodoro** → Lance/arrête Pomodoro 50min (équivaut `Win+Alt+F`)
- **Settings** → Ouvre fenêtre configuration
- **Quit** → Ferme application proprement

**Double-clic** icône système → Ouvre/focus MiniTaskbar

### Build Manuel Python

```powershell
python .\scripts\convert_logo_to_tray_icon.py --sizes 32 64 128 --input logo.png --output custom.ico
```

---

## [fa-solid fa-mobile-screen-button] Utilisation

### Lancement

**Mode GUI (par défaut):**
```powershell
.\ADHDWorkspace.exe
```

**Mode CLI:**
```powershell
# Configuration auto-start
.\ADHDWorkspace.exe autostart enable
.\ADHDWorkspace.exe autostart disable
.\ADHDWorkspace.exe autostart status

# Workspace setup
.\ADHDWorkspace.exe setup

# Save context
.\ADHDWorkspace.exe save "Nom du contexte"
```

**Options:**
- `--verbose` ou `-v` : Mode debug avec logs détaillés
- `--portable` ou `-p` : Mode portable (chemins relatifs)

### MiniTaskbar

La fenêtre principale affiche:

**Mode Collapsed (275x75px):**
```
┌─────────────────────────────────────┐
│ 14:35  ●  [fa-solid fa-bullseye]  [fa-solid fa-gear]  ×                 │
│ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬░░░░░░░░░░░      │
└─────────────────────────────────────┘
```

- **Horloge** : Heure actuelle (clic pour expand)
- **● (vert/rouge/jaune)** : Pomodoro (clic pour start/stop)
- **[fa-solid fa-bullseye]/[fa-solid fa-circle-dot]** : Focus Tracker (clic pour start/stop)
- **[fa-solid fa-gear]** : Settings
- **×** : Fermer
- **Progress bar** : Temps restant Pomodoro

**Mode Expanded (400x450px):**
Affiche en plus:
- Task description du Focus Tracker
- 8 boutons quick launch (VS Code, Teams, Outlook, Obsidian, Terminal, Edge, Save Context, Restore)

### Pomodoro

1. **Cliquer ●** dans MiniTaskbar ou `Win+Alt+F`
2. **Session 50min** démarre:
   - Focus Assist activé (mode Priority)
   - Obsidian opacité → 50%
   - Progress bar update chaque seconde
   - ● devient rouge
3. **Fin session** :
   - Toast notification "Pomodoro Complete! Time for a 10min break"
   - Focus Assist désactivé
   - Obsidian opacité → 100%
4. **Pause 10min/30min** démarre automatiquement
5. **Répète** jusqu'à stop manuel

### Focus Tracker

1. **Cliquer [fa-solid fa-bullseye]** dans MiniTaskbar ou `Win+Alt+D`
2. **Popup** demande description tâche
3. **Tracking démarre**:
   - Icône → [fa-solid fa-circle-dot]
   - Check fenêtre active chaque 15s
   - Si distraction (Chrome, Slack...) → Toast "Stay Focused! You're working on: {task}"
4. **Cliquer [fa-solid fa-circle-dot]** pour arrêter

### Quick Capture

1. **Hotkey** `Win+Shift+N`
2. **Popup** s'affiche avec Editor multi-lignes
3. **Saisir** idée/note
4. **Save** → Append à `Inbox.md` avec timestamp
5. **Toast** confirmation

### Context Save/Restore

**Save:**

1. Clic **[fa-solid fa-clipboard] Save** dans MiniTaskbar expanded ou `Win+Alt+S`
2. Popup demande nom (optionnel)
3. Sauvegarde JSON dans `context-history/`:
   - Fichiers ouverts VS Code
   - Git branch + status
   - Fenêtres actives
   - Timestamp
4. Toast confirmation avec filename

**Restore:**

1. Clic **[fa-solid fa-clipboard] Restore** dans MiniTaskbar expanded
2. Liste snapshots disponibles (CollectionView)
3. Sélection snapshot
4. Clic **Restore**
5. Ouvre VS Code avec fichiers du snapshot
6. Toast confirmation

### Settings

Clic **[fa-solid fa-gear]** dans MiniTaskbar ouvre fenêtre Settings avec 3 sections:

**General:**
- Portable Mode (switch)
- Log Level (picker: Debug/Information/Warning/Error)

**Pomodoro:**
- Focus Minutes (50)
- Short Break Minutes (10)
- Long Break Minutes (30)

**Hotkeys:**
- Quick Capture (format: Win+Shift+N)
- Start Pomodoro
- Start Focus
- Save Context
- **Validation** : Test hotkeys avant save, Toast si conflit

Boutons:
- **Cancel** : Ferme sans sauvegarder
- **Save** : Enregistre dans `appsettings.json` avec backup

## [fa-solid fa-keyboard] Hotkeys par Défaut

| Hotkey | Action | Description |
|--------|--------|-------------|
| **Win+Shift+N** | Quick Capture | Popup capture rapide Inbox.md |
| **Win+Alt+F** | Start/Stop Pomodoro | Toggle Pomodoro 50/10/30 |
| **Win+Alt+D** | Start/Stop Focus | Toggle Focus Tracker |
| **Win+Alt+S** | Save Context | Sauvegarde immédiate contexte |

*Tous configurables via Settings*

## [fa-solid fa-screwdriver-wrench] Architecture Technique

### Services

**Infrastructure:**
- `IConfigurationService` - Chargement/sauvegarde appsettings.json
- `LoggerService` - Serilog avec rotation 7 jours
- `INotificationService` - Toast notifications Windows
- `IScreenService` - Détection écran rightmost
- `IHotkeyService` - RegisterHotKey Win32 API

**Métier:**
- `PomodoroService` - Gestion cycles focus/pause avec Focus Assist
- `FocusTrackerService` - Monitoring fenêtre active, détection distractions
- `ObsidianOverlayService` - Contrôle opacité fenêtre Obsidian
- `ContextService` - Save/restore workspace VS Code + Git
- `WorkspaceSetupService` - Setup initial (masquer taskbar, lancer apps, daily note)

### Technologies

- **.NET 9** - Framework moderne
- **MAUI** - UI cross-platform Windows
- **CommunityToolkit.Maui** - Extensions UI
- **Serilog** - Logging structuré
- **Microsoft.Toolkit.Uwp.Notifications** - Toast notifications
- **xUnit** - Tests unitaires

### Structure Projet

```
ADHDWorkspace/
├── src/
│   ├── Program.cs                      # Entry point + CLI routing
│   ├── MauiProgram.cs                  # DI configuration
│   ├── App.xaml/cs                     # Application MAUI
│   ├── Commands/
│   │   ├── AutoStartCommand.cs         # Registry auto-start
│   │   ├── SaveContextCommand.cs
│   │   └── WorkspaceSetupCommand.cs
│   ├── Services/                       # 10 services
│   ├── Views/
│   │   ├── MiniTaskbarWindow.xaml/cs   # Main window
│   │   ├── SettingsPage.xaml/cs
│   │   ├── QuickCapturePage.xaml/cs
│   │   └── RestoreContextPage.xaml/cs
│   └── Models/
│       ├── AppConfig.cs                # Configuration classes
│       └── Models.cs                   # Data models
├── Platforms/Windows/
│   └── App.xaml/cs                     # WinUI bootstrap
├── config/
│   └── appsettings.json
├── tests/
│   ├── Commands/
│   └── Services/
└── docs/adr/
    ├── 0001-migration-maui-consolidation.md
    └── template.md
```

## [fa-solid fa-flask] Tests

```powershell
# Run all tests
dotnet test tests\ADHDWorkspace.Tests.csproj

# Run with coverage
dotnet test tests\ADHDWorkspace.Tests.csproj /p:CollectCoverage=true
```

**Tests inclus:**
- `AutoStartCommandTests` - Registry operations
- `ConfigurationServiceTests` - Load/save/portable mode

## [fa-solid fa-box-open] Déploiement

### Build Release

```powershell
dotnet publish ADHDWorkspace.csproj --configuration Release --framework net9.0-windows10.0.19041.0
```

Binaire: `bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\ADHDWorkspace.exe`

### Mode Portable

1. Copier dossier `publish/` vers clé USB
2. Lancer avec `--portable`:
   ```powershell
   .\ADHDWorkspace.exe --portable
   ```
3. Chemins utilisés:
   - `data/` au lieu de `C:\Temp\`
   - `config/appsettings.json` local
   - `logs/` local

### Auto-Start Windows

```powershell
# Enable
.\ADHDWorkspace.exe autostart enable

# Disable
.\ADHDWorkspace.exe autostart disable

# Check status
.\ADHDWorkspace.exe autostart status
```

Crée entrée Registry: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\ADHDWorkspace`

## [fa-solid fa-bug] Dépannage

### Hotkeys ne fonctionnent pas

**Cause**: Conflit avec autre application

**Solution**:

1. Ouvrir Settings ([fa-solid fa-gear])
2. Section Hotkeys
3. Modifier combinaison
4. Save (validation automatique Toast si conflit)

### Pomodoro ne démarre pas

**Cause**: Focus Assist déjà contrôlé par autre app

**Solution**:

1. Vérifier logs: `C:\Temp\adhd-workspace-{date}.log`
2. Désactiver Focus Assist manual dans Windows Settings
3. Relancer application

### Context Save échoue

**Cause**: VS Code non installé ou chemin invalide

**Solution**:

1. Settings → Général → Portable Mode ON
2. Ou éditer `config/appsettings.json`:
   ```json
   "VSCodePath": "C:\\Program Files\\Microsoft VS Code\\Code.exe"
   ```

### Application ne démarre pas

**Vérifier**:
```powershell
# Check runtime
dotnet --list-runtimes

# Should show: Microsoft.NETCore.App 9.0.x
```

**Installer runtime**:
```powershell
winget install Microsoft.DotNet.Runtime.9
```

## [fa-solid fa-book] Documentation

- [ADR 0001 - Migration MAUI](docs/adr/0001-migration-maui-consolidation.md)
- [Guide Configuration](GUIDE-CONFIGURATION.md)
- [Changelog](CHANGELOG.md)

## [fa-solid fa-handshake] Contribution

### Build depuis sources

```powershell
# Clone
git clone https://github.com/yourusername/adhd-workspace.git
cd adhd-workspace

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Tests

```powershell
# Run tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov
```

## [fa-solid fa-file-lines] Licence

MIT License - Libre d'utilisation et modification

---

## [fa-solid fa-square-check] Migration depuis Scripts PowerShell

Si vous utilisez ancienne version scripts:

1. **Désactiver** Task Scheduler ancien startup:
   ```powershell
   Unregister-ScheduledTask -TaskName "ADHD-WorkspaceStartup" -Confirm:$false
   ```

2. **Supprimer** dossier `scripts/` (sauvegardé dans Git)

3. **Lancer** nouvelle app:
   ```powershell
   .\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\ADHDWorkspace.exe
   ```

4. **Activer** auto-start:
   ```powershell
   .\ADHDWorkspace.exe autostart enable
   ```

**Fonctionnalités préservées:**

- [fa-solid fa-square-check] Pomodoro 50/10/30
- [fa-solid fa-square-check] Focus Tracker
- [fa-solid fa-square-check] Quick Capture
- [fa-solid fa-square-check] Context Save/Restore
- [fa-solid fa-square-check] Obsidian Overlay
- [fa-solid fa-square-check] Mini Taskbar
- [fa-solid fa-square-check] Hotkeys

**Nouvelles fonctionnalités:**

- [fa-solid fa-square-check] Settings UI
- [fa-solid fa-square-check] Progress bar visuelle
- [fa-solid fa-square-check] Toast notifications
- [fa-solid fa-square-check] Mode portable
- [fa-solid fa-square-check] Tests automatisés
- [fa-solid fa-square-check] Logging structuré
- [fa-solid fa-square-check] Configuration JSON

---

**Application ADHD Workspace créée avec [fa-solid fa-heart] en .NET MAUI 9**
