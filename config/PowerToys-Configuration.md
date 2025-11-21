# Configuration PowerToys pour Workspace ADHD

Ce document décrit la configuration PowerToys à appliquer manuellement.

## Installation PowerToys

```powershell
winget install Microsoft.PowerToys
```

## 1. FancyZones - Layouts Multi-Écrans

### Écran Portrait (Secondaire)
**Layout: Portrait-Communication**
- Type: Grid
- Rows: 2
- Columns: 1
- Zone 1 (Haut 50%): Teams PWA
- Zone 2 (Bas 50%): Outlook PWA

**Configuration JSON:**
```json
{
  "name": "Portrait-Communication",
  "type": "grid",
  "rows": 2,
  "columns": 1,
  "row-percents": [50, 50],
  "column-percents": [100],
  "zones": [
    {"row": 0, "column": 0, "rowspan": 1, "columnspan": 1},
    {"row": 1, "column": 0, "rowspan": 1, "columnspan": 1}
  ]
}
```

### Écran Paysage (Principal)
**Layout: Landscape-Dev**
- Type: Grid
- Rows: 1
- Columns: 2
- Zone 1 (Gauche 60%): VS Code / Visual Studio
- Zone 2 (Droite 40%): Terminal / Référence

**Configuration JSON:**
```json
{
  "name": "Landscape-Dev",
  "type": "grid",
  "rows": 1,
  "columns": 2,
  "row-percents": [100],
  "column-percents": [60, 40],
  "zones": [
    {"row": 0, "column": 0, "rowspan": 1, "columnspan": 1},
    {"row": 0, "column": 1, "rowspan": 1, "columnspan": 1}
  ]
}
```

### Paramètres FancyZones Recommandés
- ✅ Override Windows Snap
- ✅ Move newly created windows to last known zone
- ✅ Move newly created windows to current active monitor
- ✅ Restore the original size of windows when unsnapping
- ❌ Make dragged window transparent (réduit distraction)
- ✅ Allow zones to span across monitors: OFF
- ✅ When multiple zones overlap: Zone to which the center of the window is closest
- Space around zones: **12px**
- Zone opacity: **50%**

## 2. Keyboard Manager - Hotkeys Globaux

### Remap Shortcuts (Global)

| Hotkey | Action | Description |
|--------|--------|-------------|
| `Win + Shift + N` | `powershell.exe -WindowStyle Hidden -File "c:\WORK\Perso\adhd\scripts\QuickCapture.ps1"` | Capture rapide idée |
| `Win + Alt + F` | `powershell.exe -File "c:\WORK\Perso\adhd\scripts\DevPomodoro.ps1"` | Démarrer Pomodoro 50min |
| `Win + Alt + R` | `powershell.exe -File "c:\WORK\Perso\adhd\scripts\RestoreContext.ps1"` | Restaurer contexte |
| `Win + Alt + S` | `powershell.exe -File "c:\WORK\Perso\adhd\scripts\AutoSaveContext.ps1"` | Save contexte manuel |
| `Ctrl + Shift + Alt + C` | `ms-copilot:` | Ouvrir Copilot |
| `Win + Alt + O` | `obsidian://open?vault=adhd` | Ouvrir Obsidian |

### Configuration JSON:
```json
{
  "remapKeys": {
    "inProcess": []
  },
  "remapShortcuts": {
    "global": [
      {
        "originalKeys": "win+shift+n",
        "newRemapKeys": "",
        "runProgramFilePath": "powershell.exe",
        "runProgramArgs": "-WindowStyle Hidden -File \"c:\\WORK\\Perso\\adhd\\scripts\\QuickCapture.ps1\""
      },
      {
        "originalKeys": "win+alt+f",
        "newRemapKeys": "",
        "runProgramFilePath": "powershell.exe",
        "runProgramArgs": "-File \"c:\\WORK\\Perso\\adhd\\scripts\\DevPomodoro.ps1\""
      },
      {
        "originalKeys": "win+alt+r",
        "newRemapKeys": "",
        "runProgramFilePath": "powershell.exe",
        "runProgramArgs": "-File \"c:\\WORK\\Perso\\adhd\\scripts\\RestoreContext.ps1\""
      }
    ],
    "appSpecific": []
  }
}
```

## 3. PowerToys Run

### Paramètres Recommandés
- Activation shortcut: `Alt + Space`
- Preferred monitor: **Display with mouse cursor**
- Clear the previous query on launch: ✅
- Theme: **Match Windows**

### Plugins Activés
- ✅ Program (Applications)
- ✅ Window Walker (Switch windows)
- ✅ Calculator
- ✅ System (Commands)
- ❌ Web Search (réduit distractions)
- ❌ Browser Bookmarks (réduit distractions)

## 4. Always on Top

### Paramètres
- Activation shortcut: `Win + Ctrl + T`
- Border color: `#333333` (gris foncé)
- Border opacity: **30%**
- Border thickness: **2px**
- Sound: ❌ Désactivé
- Excluded apps: (vide)

## 5. Color Picker

Désactivé par défaut (non nécessaire pour workflow ADHD)

## 6. PowerToys Awake

### Configuration
- Mode: **Keep screen on**
- Keep screen on indefinitely: ❌
- Timed mode: ✅
  - Hours: 8 (pendant journée de travail)

## Installation des Configurations

### Méthode 1: Import Manuel
1. Ouvrir PowerToys Settings
2. Aller dans chaque module (FancyZones, Keyboard Manager)
3. Importer ou configurer manuellement selon les valeurs ci-dessus

### Méthode 2: Script PowerShell
```powershell
# Copier les fichiers de configuration
$powerToysConfig = "$env:LOCALAPPDATA\Microsoft\PowerToys"

# FancyZones
Copy-Item ".\config\fancyzones-settings.json" "$powerToysConfig\FancyZones\settings.json" -Force

# Keyboard Manager
Copy-Item ".\config\keyboard-manager.json" "$powerToysConfig\Keyboard Manager\default.json" -Force

# Redémarrer PowerToys
Stop-Process -Name "PowerToys" -Force
Start-Process "C:\Program Files\PowerToys\PowerToys.exe"
```

## Vérification Configuration

Après installation, vérifier:

1. **FancyZones**: Win+Shift+` pour afficher zones
2. **Keyboard Manager**: Tester Win+Shift+N pour quick capture
3. **Always on Top**: Tester Win+Ctrl+T sur fenêtre Obsidian
4. **PowerToys Run**: Tester Alt+Space

## Dépannage

### FancyZones ne fonctionne pas
- Vérifier que PowerToys s'exécute en tant qu'administrateur
- Redémarrer Explorer: `Stop-Process -Name explorer -Force`

### Hotkeys ne fonctionnent pas
- Vérifier conflits avec autres applications
- Réinstaller Keyboard Manager configuration
- Redémarrer PowerToys

### Obsidian toujours on top ne persiste pas
- Utiliser le script ObsidianOverlay.ps1 au démarrage
- Vérifier que PowerToys est configuré pour démarrer avec Windows

## Backup Configuration

Pour sauvegarder votre configuration PowerToys:

```powershell
# Exporter configuration
$backupPath = "c:\WORK\Perso\adhd\config\powertoys-backup"
New-Item -ItemType Directory -Path $backupPath -Force

Copy-Item "$env:LOCALAPPDATA\Microsoft\PowerToys" -Destination $backupPath -Recurse -Force

Write-Host "PowerToys configuration backed up to: $backupPath"
```

---

**Note**: Les fichiers JSON de configuration seront créés automatiquement lors de la première utilisation de PowerToys. Modifiez-les ensuite selon les valeurs ci-dessus.
