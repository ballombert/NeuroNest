# Workspace ADHD - Organisation Dual-Screen Optimisée

> **Environnement Windows 11 ultra-automatisé pour développeurs avec TDAH**  
> Configuration dual-screen (paysage + portrait) avec overlay Obsidian transparent, Pomodoro 50min, capture rapide, auto-save contexte, et minimalisme visuel.

## [fa-solid fa-clipboard-list] Table des Matières

- [Vue d'Ensemble](#vue-densemble)
- [Architecture](#architecture)
- [Installation](#installation)
- [Utilisation Quotidienne](#utilisation-quotidienne)
- [Hotkeys Essentiels](#hotkeys-essentiels)
- [Scripts Disponibles](#scripts-disponibles)
- [Configuration PowerToys](#configuration-powertoys)
- [Obsidian Setup](#obsidian-setup)
- [Dépannage](#dépannage)

---

## [fa-solid fa-bullseye] Vue d'Ensemble

Système complet d'organisation workspace pour développeurs C# avec TDAH, conçu pour:

- [fa-solid fa-square-check] **Réduire distractions visuelles** (barre des tâches masquée, transparence adaptative)
- [fa-solid fa-square-check] **Déporter charge mentale** (auto-save contexte, capture rapide idées)
- [fa-solid fa-square-check] **Optimiser concentration** (Pomodoro 50min, Focus Assist automatique)
- [fa-solid fa-square-check] **Organisation spatiale cohérente** (FancyZones, overlay Obsidian)
- [fa-solid fa-square-check] **Automatisation totale** (startup script, Task Scheduler)

### Configuration Écrans

**Écran Portrait (Secondaire):**
- Teams PWA (haut 50%)
- Outlook PWA (bas 50%)
- Obsidian overlay transparent (droite, 600-700px)

**Écran Paysage (Principal):**
- VS Code / Visual Studio (gauche 60%)
- Terminal / Référence (droite 40%)
- Mini Taskbar (coin inférieur droit, auto-hide)

---

## [fa-solid fa-diagram-project] Architecture

```
c:\WORK\Perso\adhd\
│
├── scripts\
│   ├── WorkspaceSetup.ps1        # [fa-solid fa-rocket] Startup automation principal
│   ├── ObsidianOverlay.ps1       # [fa-solid fa-eye] Overlay transparent adaptatif
│   ├── QuickCapture.ps1          # [fa-solid fa-bolt] Capture rapide idées
│   ├── MiniTaskbar.ps1           # [fa-solid fa-chart-column] Taskbar minimal + countdown
│   ├── DevPomodoro.ps1           # [fa-solid fa-stopwatch] Timer Pomodoro 50min
│   ├── AutoSaveContext.ps1       # [fa-solid fa-floppy-disk] Sauvegarde contexte auto
│   └── RestoreContext.ps1        # [fa-solid fa-arrows-rotate] Recovery contexte manuel
│
├── notes\                         # [fa-solid fa-note-sticky] Vault Obsidian
│   ├── Daily\                     # Notes quotidiennes
│   ├── Templates\                 # Templates (Daily, Task, QuickNote)
│   └── Inbox.md                   # Capture rapide centralisée
│
├── context-history\               # [fa-solid fa-folder-open] Backups contexte JSON
│   └── YYYY-MM-DD_HHmm.json
│
├── config\
│   └── PowerToys-Configuration.md # [fa-solid fa-gear] Config PowerToys détaillée
│
└── README.md                      # [fa-solid fa-book-open] Ce fichier
```

---

## [fa-solid fa-rocket] Installation

### Prérequis

1. **Windows 11** (version récente avec toast notifications)
2. **PowerToys** installé
3. **Obsidian** installé
4. **VS Code** installé
5. **Edge** (pour Teams/Outlook PWA)
6. **PowerShell 5.1+**

### Étape 1: Cloner/Télécharger le Workspace

```powershell
# Le workspace est déjà créé à: c:\WORK\Perso\adhd\
cd c:\WORK\Perso\adhd
```

### Étape 2: Installer PowerToys

```powershell
winget install Microsoft.PowerToys
```

### Étape 3: Configurer PowerToys

Suivre la documentation détaillée: [`config\PowerToys-Configuration.md`](config/PowerToys-Configuration.md)

**Configuration rapide:**
1. Ouvrir PowerToys Settings
2. **FancyZones**: Créer layouts Portrait-Communication + Landscape-Dev
3. **Keyboard Manager**: Configurer hotkeys globaux (voir section Hotkeys)
4. **Always on Top**: Activer avec `Win+Ctrl+T`

### Étape 4: Installer Module PowerShell BurntToast

```powershell
Install-Module -Name BurntToast -Scope CurrentUser -Force
```

### Étape 5: Configurer Obsidian

1. Créer vault Obsidian pointant vers `c:\WORK\Perso\adhd\notes\`
2. Installer plugins communautaires recommandés:
   - **Tasks** (task management)
   - **Dataview** (dashboards dynamiques)
   - **Todoist Sync** (si utilisation Todoist)
   - **Calendar** (navigation daily notes)
   - **QuickAdd** (capture rapide avancée)
3. Appliquer thème **Minimal** ou **Things** (réduction distraction)
4. Paramètres Obsidian:
   - Désactiver animations (`Settings → Appearance → Reduce animations`)
   - Activer Reading mode par défaut

### Étape 6: Installer Teams & Outlook PWA

```powershell
# Ouvrir Edge
start msedge.exe

# Naviguer vers:
# https://teams.microsoft.com → Menu (⋯) → Applications → Installer cette application
# https://outlook.office.com → Menu (⋯) → Applications → Installer cette application
```

### Étape 7: Créer Task Scheduler pour Startup

```powershell
# Exécuter en tant qu'Administrateur
$action = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-ExecutionPolicy Bypass -WindowStyle Hidden -File `"c:\WORK\Perso\adhd\scripts\WorkspaceSetup.ps1`""

$trigger = New-ScheduledTaskTrigger -AtLogOn

$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable

$principal = New-ScheduledTaskPrincipal -UserId "$env:USERDOMAIN\$env:USERNAME" `
    -LogonType Interactive -RunLevel Highest

Register-ScheduledTask -TaskName "ADHD-WorkspaceStartup" `
    -Action $action -Trigger $trigger -Settings $settings -Principal $principal `
    -Description "Auto-setup ADHD workspace at login"

Write-Host "Startup task registered successfully!" -ForegroundColor Green
```

### Étape 8: Test Manuel

```powershell
# Tester startup (sans redémarrer)
cd c:\WORK\Perso\adhd\scripts
.\WorkspaceSetup.ps1
```

---

## [fa-solid fa-calendar-days] Utilisation Quotidienne

### Démarrage Matin

1. **Connexion Windows** → Startup automatique lance tout
2. **Attendre 15-20 secondes** → Applications se positionnent automatiquement
3. **Vérifier Obsidian overlay** (droite portrait, transparent 80%)
4. **Mini Taskbar visible** (coin bas-droit paysage, hover pour afficher)

### Workflow Développement

1. **Démarrer Pomodoro** : `Win+Alt+F`
   - 50min focus → Obsidian passe à 50% opacité
   - Focus Assist activé (seulement mentions directes)
   - Auto-save contexte au début/fin
2. **Capture idée urgente** : `Win+Shift+N`
   - Popup input box instantané
   - Sauvegardé dans `Inbox.md`
3. **Changement tâche** : `Win+Alt+S` (save contexte manuel)
4. **Interruption/Pause** : Contexte auto-sauvé toutes les 10min
5. **Reprise après interruption** : `Win+Alt+R` (restore contexte)

### Fin de Journée

1. **Brain Dump** : Ouvrir daily note Obsidian, section "Brain Dump"
2. **Traiter Inbox** : Transformer idées en tâches ou notes
3. **Review contexte** : Vérifier `context-history\` pour retrouver travail

---

## [fa-solid fa-keyboard] Hotkeys Essentiels

| Hotkey | Action | Description |
|--------|--------|-------------|
| **Win+Shift+N** | Quick Capture | Popup input pour capturer idée rapidement |
| **Win+Alt+F** | Start Pomodoro | Lancer cycles 50min focus + pauses |
| **Win+Alt+R** | Restore Context | Menu sélection snapshots contexte sauvés |
| **Win+Alt+S** | Save Context | Sauvegarde manuelle contexte immédiat |
| **Win+Ctrl+T** | Toggle Always-On-Top | Activer/désactiver always-on-top (PowerToys) |
| **Win+Ctrl+O** | Toggle Obsidian | Ouvrir/focus Obsidian |
| **Win+T** | Show Taskbar | Afficher temporairement barre des tâches Windows |
| **Alt+Space** | PowerToys Run | Launcher rapide applications |
| **Ctrl+Shift+Alt+C** | Copilot Context | Ouvrir Copilot avec contexte |

### FancyZones Snapping

| Hotkey | Action |
|--------|--------|
| **Win+↑** | Snap zone haut |
| **Win+↓** | Snap zone bas |
| **Win+←** | Snap zone gauche |
| **Win+→** | Snap zone droite |
| **Win+Shift+`** | Afficher zones overlay |

---

## [fa-solid fa-screwdriver-wrench] Scripts Disponibles

### 1. WorkspaceSetup.ps1
**Startup automatique complet**

```powershell
.\scripts\WorkspaceSetup.ps1
```

- Masque barre des tâches Windows
- Lance Teams, Outlook, VS Code, Obsidian
- Applique FancyZones layouts
- Active Obsidian overlay transparent
- Lance Mini Taskbar + Auto-save contexte
- Crée daily note du jour

### 2. ObsidianOverlay.ps1
**Overlay transparent adaptatif**

```powershell
.\scripts\ObsidianOverlay.ps1 -Width 700 -Height 1800
```

- Positionne Obsidian côté droit portrait
- Always-on-top persistant
- Opacité adaptative:
  - 100% quand actif
  - 80% en arrière-plan
  - 50% pendant Pomodoro

### 3. QuickCapture.ps1
**Capture rapide idées**

```powershell
.\scripts\QuickCapture.ps1
```

- Affiche popup input Windows Forms
- Sauvegarde dans `Inbox.md` format libre
- Workflow 3-5 secondes max

### 4. MiniTaskbar.ps1
**Taskbar minimaliste + countdown**

```powershell
.\scripts\MiniTaskbar.ps1
```

- Barre verticale coin bas-droit
- Auto-hide au survol
- Horloge + countdown Pomodoro dynamique
- 4 icônes quick launch (VS Code, Teams, Outlook, Obsidian)

### 5. DevPomodoro.ps1
**Timer Pomodoro 50min**

```powershell
.\scripts\DevPomodoro.ps1 -FocusMinutes 50 -ShortBreakMinutes 10
```

- Cycles: 50min focus + 10min pause + 30min longue pause (tous les 4 cycles)
- Contrôle Focus Assist automatique
- Update Obsidian opacité (50% pendant focus)
- Auto-save contexte début/fin sessions
- Update Mini Taskbar countdown

### 6. AutoSaveContext.ps1
**Sauvegarde auto contexte**

```powershell
.\scripts\AutoSaveContext.ps1
```

- Capture fichiers ouverts VS Code
- Git status (branch, uncommitted changes)
- Fenêtres actives
- Export JSON horodaté dans `context-history\`
- Nettoyage automatique (>7 jours)

### 7. RestoreContext.ps1
**Recovery contexte manuel**

```powershell
.\scripts\RestoreContext.ps1
```

- Menu Out-GridView avec snapshots disponibles
- Affiche détails complet contexte
- Options: Restore VS Code workspace, Navigate directory, Export report
- Restauration complète automatique

### 8. Build Automation (Cake)

**Automatisation génération icône système depuis logo**

```powershell
# Regenerate tray icon uniquement
.\build.ps1 --target GenerateTrayIcon

# Build complet projet (inclut régénération icône)
.\build.ps1 --target Build
```

**Prérequis:**
- **Python 3.7+** (`python --version`)
- **Pillow** (`pip install pillow`)

**Fonctionnement:**
1. `build.ps1` bootstrap Cake.Tool (installation auto si absent)
2. Cake valide Python + Pillow disponibles
3. Script Python `convert_logo_to_tray_icon.py` convertit `Resources/Images/Logo.png` → `Resources/Images/trayicon.ico`
4. Génère multi-résolution ICO (16x16, 32x32, 48x48, 256x256)
5. `TrayIconService` charge nouveau `.ico` au prochain lancement app

**Commande Python directe (optionnel):**
```powershell
python .\scripts\convert_logo_to_tray_icon.py --sizes 32 64 128 --input custom-logo.png --output custom-tray.ico
```

**Structure Menu Tray:**
- **Start Pomodoro** → Lance/stoppe cycles 50min focus (équivaut `Win+Alt+F`)
- **Settings** → Ouvre page configuration UI
- **Quit** → Ferme application proprement (dispose tray icon, services)

**Double-clic icône** système → Ouvre/focus MiniTaskbar window

---

## [fa-solid fa-gear] Configuration PowerToys

Voir documentation complète: [`config\PowerToys-Configuration.md`](config/PowerToys-Configuration.md)

### Configuration Rapide FancyZones

**Écran Portrait:**
```
┌─────────────┐
│   Teams     │ 50%
├─────────────┤
│   Outlook   │ 50%
└─────────────┘
```

**Écran Paysage:**
```
┌──────────────┬──────┐
│   VS Code    │ Term │
│     60%      │ 40%  │
└──────────────┴──────┘
```

---

## [fa-solid fa-note-sticky] Obsidian Setup

### Plugins Requis (Community)

1. **Tasks** - Gestion tâches avec due dates
2. **Dataview** - Dashboards dynamiques
3. **Todoist Sync** - Intégration Todoist (optionnel)
4. **Calendar** - Navigation daily notes
5. **QuickAdd** - Capture avancée (optionnel)

### Templates Disponibles

- **Daily.md** - Note quotidienne avec sections Focus, Inbox, Completed
- **Task.md** - Tâche structurée avec steps, context
- **QuickNote.md** - Note capture rapide minimaliste

### Configuration Recommandée

```
Settings → Appearance:
- Theme: Minimal
- Base theme: Dark (réduit fatigue visuelle)
- Reduce animations: [fa-solid fa-square-check]
- Native menus: [fa-solid fa-square-check]

Settings → Editor:
- Default view for new tabs: Reading view
- Show line numbers: [fa-solid fa-square-xmark]
- Readable line length: [fa-solid fa-square-check]

Settings → Hotkeys:
- Toggle reading view: Ctrl+E
- Open daily note: Ctrl+D
- Search: Ctrl+Shift+F
```

---

## [fa-solid fa-bug] Dépannage

### Problème: Startup script ne lance pas applications

**Solution:**
```powershell
# Vérifier Task Scheduler
Get-ScheduledTask -TaskName "ADHD-WorkspaceStartup"

# Tester manuellement avec log
.\scripts\WorkspaceSetup.ps1 *> startup-log.txt
```

### Problème: Obsidian overlay pas transparent

**Solution:**
```powershell
# Relancer script overlay
.\scripts\ObsidianOverlay.ps1

# Vérifier process Obsidian
Get-Process -Name "Obsidian" | Select-Object MainWindowHandle, MainWindowTitle
```

### Problème: FancyZones ne snap pas correctement

**Solution:**
1. Ouvrir PowerToys Settings
2. FancyZones → Edit zones → Vérifier layouts
3. `Win+Shift+`}` pour afficher zones
4. Redémarrer Explorer: `Stop-Process -Name explorer -Force`

### Problème: Hotkeys ne fonctionnent pas

**Solution:**
```powershell
# Vérifier PowerToys Keyboard Manager
Get-Process -Name "PowerToys*"

# Redémarrer PowerToys
Stop-Process -Name "PowerToys" -Force
Start-Process "C:\Program Files\PowerToys\PowerToys.exe"
```

### Problème: Mini Taskbar ne s'affiche pas

**Solution:**
```powershell
# Vérifier processus
Get-Process -Name "powershell" | Where-Object { $_.MainWindowTitle -match "Mini" }

# Relancer
.\scripts\MiniTaskbar.ps1
```

### Problème: Context auto-save ne fonctionne pas

**Solution:**
```powershell
# Vérifier Task Scheduler
Get-ScheduledTask -TaskName "ADHD-ContextAutoSave"

# Tester manuellement
.\scripts\AutoSaveContext.ps1

# Vérifier historique
Get-ChildItem c:\WORK\Perso\adhd\context-history\
```

---

## [fa-solid fa-palette] Personnalisation

### Modifier Dimensions Overlay Obsidian

```powershell
# Éditer ObsidianOverlay.ps1
# Ligne 15-16:
param(
    [int]$Width = 700,   # Ajuster largeur (600-800 recommandé)
    [int]$Height = 1800  # Ajuster hauteur selon écran
)
```

### Modifier Durées Pomodoro

```powershell
# Éditer DevPomodoro.ps1
# Ligne 16-18:
param(
    [int]$FocusMinutes = 50,        # Durée focus
    [int]$ShortBreakMinutes = 10,   # Pause courte
    [int]$LongBreakMinutes = 30     # Pause longue
)
```

### Changer Position Mini Taskbar

```powershell
# Éditer MiniTaskbar.ps1
# Ligne 46-49:
$form.Location = New-Object System.Drawing.Point(
    ($screen.WorkingArea.Width - $Width - 10),  # X: droite
    ($screen.WorkingArea.Height - $Height - 10) # Y: bas
)
# Modifier X et Y pour repositionner
```

---

## [fa-solid fa-books] Ressources Supplémentaires

- [PowerToys Documentation](https://learn.microsoft.com/windows/powertoys/)
- [Obsidian Documentation](https://help.obsidian.md/)
- [ADHD & Productivity](https://www.additudemag.com/)
- [Pomodoro Technique](https://francescocirillo.com/pages/pomodoro-technique)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/what-is-maui)

---

## [fa-solid fa-handshake] Support & Contribution

Pour questions, suggestions ou améliorations:

- Créer issues dans le repository
- Adapter scripts selon besoins personnels
- Partager configurations PowerToys personnalisées

---

## [fa-solid fa-file-lines] Licence

Scripts fournis "as-is" pour usage personnel. Modifier et adapter selon vos besoins.

- **Font Awesome Free** – Icônes sous licence [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) et polices sous [SIL Open Font License 1.1](https://scripts.sil.org/OFL). Attribution requise : "Icons by Font Awesome – [fontawesome.com](https://fontawesome.com)".
- Voir `THIRD-PARTY-NOTICES.md` pour le détail des licences tierces incluses.

---

## [fa-solid fa-square-check] Checklist Installation Complète

- [ ] PowerToys installé et configuré
- [ ] BurntToast module installé
- [ ] Obsidian configuré avec vault `c:\WORK\Perso\adhd\notes\`
- [ ] Teams PWA installé
- [ ] Outlook PWA installé
- [ ] VS Code installé
- [ ] Task Scheduler configuré pour startup
- [ ] Hotkeys PowerToys testés
- [ ] FancyZones layouts créés
- [ ] Test manuel `WorkspaceSetup.ps1` réussi
- [ ] Obsidian overlay fonctionne avec transparence
- [ ] Mini Taskbar affichée avec countdown
- [ ] Quick Capture testé (`Win+Shift+N`)
- [ ] Pomodoro testé (`Win+Alt+F`)
- [ ] Context restore testé (`Win+Alt+R`)

---

**Workspace optimisé TDAH créé avec [fa-solid fa-heart] pour développeurs C# sous Windows 11**
