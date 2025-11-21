# Guide Configuration PowerToys - Étape par Étape

## [fa-solid fa-clipboard-list] Checklist Configuration

### [fa-solid fa-square-check] Étape 1: FancyZones (Layouts Écrans)

1. **Ouvrir PowerToys Settings** (déjà ouvert)
2. Cliquer sur **FancyZones** dans le menu gauche
3. Activer **Enable FancyZones** (toggle ON)

#### Configuration Layout Écran Portrait

1. Cliquer sur **"Launch layout editor"**
2. Sélectionner votre **écran portrait** (moniteur secondaire)
3. Cliquer **"+ Create new layout"**
4. Choisir **"Grid"**
5. Configurer:
   - **Name**: `Portrait-Communication`
   - **Columns**: `1`
   - **Rows**: `2`
   - **Show space around zones**: `12` (px)
6. Cliquer **"Save and apply"**

**Résultat attendu:**
```
┌─────────────┐
│   Zone 1    │  ← Teams (50%)
│   (Haut)    │
├─────────────┤
│   Zone 2    │  ← Outlook (50%)
│   (Bas)     │
└─────────────┘
```

#### Configuration Layout Écran Paysage

1. Dans **Layout editor**, sélectionner votre **écran paysage** (moniteur principal)
2. Cliquer **"+ Create new layout"**
3. Choisir **"Grid"**
4. Configurer:
   - **Name**: `Landscape-Dev`
   - **Columns**: `2`
   - **Rows**: `1`
   - **Column 1 width**: `60%`
   - **Column 2 width**: `40%`
   - **Show space around zones**: `12` (px)
5. Cliquer **"Save and apply"**

**Résultat attendu:**
```
┌──────────────────┬───────────┐
│   Zone 1         │  Zone 2   │
│   VS Code 60%    │  Term 40% │
└──────────────────┴───────────┘
```

#### Paramètres FancyZones Additionnels

Retourner dans **FancyZones Settings** et configurer:

- [fa-solid fa-square-check] **Override Windows Snap**: ON
- [fa-solid fa-square-check] **Move newly created windows to their last known zone**: ON
- [fa-solid fa-square-check] **Move newly created windows to the current active monitor**: ON
- [fa-solid fa-square-check] **Restore the original size of windows when unsnapping**: ON
- [fa-solid fa-xmark] **Make dragged window transparent**: OFF (réduit distraction)
- **Zone highlight opacity (%)**: `50`
- **Excluded apps**: (laisser vide)

---

### [fa-solid fa-square-check] Étape 2: Keyboard Manager (Hotkeys)

1. Cliquer sur **Keyboard Manager** dans le menu gauche
2. Activer **Enable Keyboard Manager**: ON
3. Cliquer sur **"Remap a shortcut"**

#### Ajouter les Hotkeys (un par un)

**Hotkey 1: Quick Capture**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Win + Shift + N`
- **Mapped to**: Sélectionner "Run Program"
- **App**: `powershell.exe`
- **Args**: `-WindowStyle Hidden -ExecutionPolicy Bypass -File "c:\WORK\Perso\adhd\scripts\QuickCapture.ps1"`
- **Start in**: `c:\WORK\Perso\adhd\scripts`
- Cliquer **OK**

**Hotkey 2: Pomodoro Timer**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Win + Alt + F`
- **Mapped to**: "Run Program"
- **App**: `powershell.exe`
- **Args**: `-ExecutionPolicy Bypass -File "c:\WORK\Perso\adhd\scripts\DevPomodoro.ps1"`
- **Start in**: `c:\WORK\Perso\adhd\scripts`
- Cliquer **OK**

**Hotkey 3: Restore Context**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Win + Alt + R`
- **Mapped to**: "Run Program"
- **App**: `powershell.exe`
- **Args**: `-ExecutionPolicy Bypass -File "c:\WORK\Perso\adhd\scripts\RestoreContext.ps1"`
- **Start in**: `c:\WORK\Perso\adhd\scripts`
- Cliquer **OK**

**Hotkey 4: Save Context Manuel**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Win + Alt + S`
- **Mapped to**: "Run Program"
- **App**: `powershell.exe`
- **Args**: `-WindowStyle Hidden -ExecutionPolicy Bypass -File "c:\WORK\Perso\adhd\scripts\AutoSaveContext.ps1"`
- **Start in**: `c:\WORK\Perso\adhd\scripts`
- Cliquer **OK**

**Hotkey 5: Ouvrir Obsidian**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Win + Alt + O`
- **Mapped to**: "Run Program"
- **App**: `obsidian.exe`
- **Args**: `obsidian://open?vault=adhd`
- Cliquer **OK**

**Hotkey 6: Copilot Context**
- Click **"+ Add shortcut remapping"**
- **Shortcut**: Appuyer `Ctrl + Shift + Alt + C`
- **Mapped to**: "Open URI"
- **URI**: `ms-copilot:`
- Cliquer **OK**

Cliquer **"Continue Anyway"** si warning sur conflits.

---

### [fa-solid fa-square-check] Étape 3: Always on Top

1. Cliquer sur **Always on Top** dans le menu gauche
2. Activer **Enable Always on Top**: ON
3. Configurer:
   - **Activation shortcut**: `Win + Ctrl + T` (par défaut)
   - **Border color**: Cliquer sur couleur → choisir gris foncé `#333333`
   - **Border thickness**: `2` (px)
   - **Border opacity (%)**: `30`
   - **Sound**: [fa-solid fa-xmark] OFF
   - **Excluded apps**: (laisser vide)

---

### [fa-solid fa-square-check] Étape 4: PowerToys Run

1. Cliquer sur **PowerToys Run** dans le menu gauche
2. Activer **Enable PowerToys Run**: ON
3. Configurer:
   - **Activation shortcut**: `Alt + Space` (par défaut)
   - **Preferred monitor position**: "Focus"
   - **Clear the previous query on launch**: [fa-solid fa-square-check] ON
   - **Theme**: "Windows default"

#### Plugins à Activer/Désactiver

Dans **Plugins** section:

- [fa-solid fa-square-check] **Program**: ON (lancer applications)
- [fa-solid fa-square-check] **Window Walker**: ON (switch windows)
- [fa-solid fa-square-check] **Calculator**: ON
- [fa-solid fa-square-check] **System**: ON (commands)
- [fa-solid fa-xmark] **Web Search**: OFF (réduit distractions)
- [fa-solid fa-xmark] **Browser Bookmarks**: OFF
- [fa-solid fa-xmark] **Unit Converter**: OFF (optionnel)
- [fa-solid fa-square-check] **Shell**: ON (commandes PowerShell)

---

### [fa-solid fa-square-check] Étape 5: Vérification Configuration

#### Test FancyZones

1. Appuyer `Win + Shift + \`` (backtick) → Affiche zones overlay
2. Vérifier que zones portrait et paysage sont correctes

#### Test Keyboard Manager

1. Appuyer `Win + Shift + N` → Doit ouvrir popup Quick Capture
2. Appuyer `Win + Alt + O` → Doit ouvrir/focus Obsidian
3. Appuyer `Alt + Space` → Doit ouvrir PowerToys Run

#### Test Always on Top

1. Ouvrir une fenêtre (ex: Notepad)
2. Appuyer `Win + Ctrl + T` → Bordure grise doit apparaître
3. Cliquer autre fenêtre → Notepad reste au-dessus

---

## [fa-solid fa-screwdriver-wrench] Configuration Additionnelle

### Masquer Barre des Tâches Windows

**Méthode manuelle:**

1. Clic-droit sur barre des tâches
2. **Paramètres de la barre des tâches**
3. Activer **"Masquer automatiquement la barre des tâches"**
4. Appliquer sur les deux écrans

**OU via script** (déjà inclus dans WorkspaceSetup.ps1)

---

## [fa-solid fa-square-check] Test Final

Exécuter le test complet:

```powershell
# Test startup complet
cd c:\WORK\Perso\adhd\scripts
.\WorkspaceSetup.ps1
```

**Vérifications:**

- [ ] PowerToys FancyZones actif
- [ ] Hotkeys fonctionnent (Win+Shift+N, Win+Alt+F, etc.)
- [ ] Always on Top fonctionne (Win+Ctrl+T)
- [ ] PowerToys Run fonctionne (Alt+Space)
- [ ] Barre des tâches masquée

---

## [fa-solid fa-camera] Captures d'Écran Attendues

**FancyZones Editor:**

- 2 layouts configurés (Portrait-Communication, Landscape-Dev)
- Zones visibles avec overlay `Win+Shift+\``

**Keyboard Manager:**

- 6 raccourcis configurés
- Tous avec status "OK" (pas d'erreur)

**Always on Top:**

- Bordure grise 2px avec opacité 30%
- Toggle avec Win+Ctrl+T

---

## [fa-solid fa-circle-question] Besoin d'Aide ?

Si un hotkey ne fonctionne pas:

1. Vérifier que le chemin du script est correct
2. Tester le script manuellement: `.\scripts\QuickCapture.ps1`
3. Redémarrer PowerToys

Si FancyZones ne snap pas:

1. Vérifier "Override Windows Snap" est ON
2. Redémarrer Explorer: `Stop-Process -Name explorer -Force`
3. Réouvrir Layout Editor et vérifier zones

---

**Configuration PowerToys terminée ! Vous pouvez maintenant passer à l'étape 4 (Obsidian vault).**
