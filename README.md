# Manager for Omron FH Vision System Simulators

![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=flat&logo=windows)
![License](https://img.shields.io/badge/License-GPLv3-blue.svg)
![Version](https://img.shields.io/badge/Version-1.0.0-green.svg)

This tool is a resource-saving, lightweight open-source tool for Windows. It simplifies the management of Omron FH Simulators. 

## 📖 About the Project

This tool reduces copy errors, makes swapping active simulators significantly faster, and, depending on the settings, optionally reduces duplicate copies of USB Disks.

### ✨ Key Features
* **Structured Overview:** It provides a nicely structured overview of the available Simulators and USB Disks.
* **Fast Swapping:** In addition, active USB Disks or Simulators can be swapped out very easily.
* **Easy Backups:** Local Simulators and USB Disks can also be easily backed up to a network drive.
* **Clear Tracking:** This also makes it very easy to see exactly when the active Simulator or USB Disk was last backed up.

---

## ⚠️ Note on the Windows SmartScreen Warning

When you launch the setup, Windows might display a blue warning screen ("Windows protected your PC"). 
This happens because this is a free open-source project and I haven't purchased an expensive developer certificate to digitally sign the `.exe` file.

**Here is how you can install the app anyway:**
1. Click on **More info** in the blue window.
2. Click the **Run anyway** button.
3. Since the entire source code is transparently available here on GitHub, you can rest assured.

---

## 🚀 First Use

### 1. Simulator Folder
For the first use, you need a standard Simulator; this is currently the standard Omron Simulator folder: `C:\Users\GB89\Documents\OMRON FZ`.
This folder typically contains the following subfolders:
* `MED-Package`
* `RAMDisk`
* `SettingData`
* `USBDisk`

### 2. Set the paths first
Configure your standard paths in the settings:
* **Simulator Path:** The path to your active simulator (e.g., `C:\Users\GB89\Documents\OMRON FZ`).
* **Simulators Path:** The directory for storing your different simulators (e.g., `C:\Users\GB89\Documents\OMRON_FZ_Simulators`).
* **Simulators Backup Path:** The target path for your simulator backups (e.g., `B:\OMRON_FZ_Simulator_Backups`).
* **USB Disk Path:** The directory for your USB disks (e.g., `C:\Users\GB89\Documents\OMRON_FZ_USBDisks`).
* **USB Disk Backup Path:** The target path for your USB disk backups (e.g., `B:\OMRON_FZ_Simulator_USBDiskBackups`).

### 3. Set "Types of administration"
You have two options, but they are not 100% compatible with each other!
* **Mode 0: Simulator and USBDisk is same.** With this setting, the USB Disk is always kept together with the Simulator. When the Simulator is loaded, the identically named USB Disk is also searched for and loaded.
* **Mode 1: One USBDisk can use for more once Simulator and you can load Items separately.** To save storage space and loading times, the Simulator and USB Disk can be loaded separately using the "Load Item" button. If a Simulator should be loaded together with its linked USB Disk, you can simply use the "Load" button. Likewise, the Simulator and USB Disk can be backed up individually.

### 4. No name for active items
Assign a name to each and press save; this will create the first item in the list.

---

## ❓ FAQ & Troubleshooting

**How to create a new Simulator or USBDisk?** To create a new Simulator or USB Disk, simply rename the currently loaded Simulator or USB Disk and then click a save button to store it. Files will only be overwritten if they already existed.

**How does the app know the name of the Simulator or USB Disk?** * A `*.gb` file is always created in the Simulator folder, and the name of the linked USB Disk is written inside it.
* A `*.gb` file is also created in the USB Disk folder, but it remains empty.

> ⚠️ **Warning:** Do not delete these files and, if possible, do not modify them!

---

## 🐛 Bug Reporting & Feature Requests

Found a bug or have a great idea for a new feature? 
The best way to let me know is by opening a new Issue right here in the "Issues" tab on GitHub. Alternatively, feel free to reach out directly!

---

## 📄 License & Author

* **Author:** Designed and Created by **Georg Black**.
* **License:** This app is an open-source project released under the **GNU General Public License v3.0 (GPLv3)**. This means you are free to use, modify, and redistribute the software, provided that all modifications remain open source under the same license. For more details on the licensing terms, please refer to the `LICENSE` file in this repository.

# Manager for Omron FH Vision System Simulators

![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?style=flat&logo=windows)
![License](https://img.shields.io/badge/License-GPLv3-blue.svg)
![Version](https://img.shields.io/badge/Version-1.0.0-green.svg)

Ein ressourcenschonendes, leichtgewichtiges Open-Source-Tool für Windows zur einfachen Verwaltung von Omron FH Simulatoren. 

## 📖 Über das Projekt (About)

Dieses Tool vereinfacht die Arbeit mit Omron FH Simulatoren erheblich und reduziert Kopierfehler. Das Wechseln zwischen aktiven Simulatoren wird deutlich beschleunigt, und je nach Einstellung lässt sich die Anzahl duplizierter USB-Disks optimieren.

### ✨ Hauptfunktionen
* **Übersichtliche Struktur:** Bietet eine klar strukturierte Übersicht aller verfügbaren Simulatoren und USB-Disks.
* **Schneller Wechsel:** Aktive Simulatoren und USB-Disks lassen sich mit wenigen Klicks austauschen.
* **Einfaches Backup:** Lokale Simulators und USB-Disks können problemlos auf ein Netzwerklaufwerk gesichert werden.
* **Transparenz:** Es ist auf einen Blick ersichtlich, wann genau der aktive Simulator oder die USB-Disk zuletzt gesichert wurde.

---

## ⚠️ Wichtiger Hinweis zur Windows SmartScreen-Warnung

Wenn du das Setup ausführst, zeigt Windows möglicherweise einen blauen Warnbildschirm an ("Der Windows-PC wurde durch Windows geschützt"). 
Dies geschieht, da es sich um ein kostenloses Open-Source-Projekt handelt und kein teures Entwicklerzertifikat zur digitalen Signatur der `.exe`-Datei erworben wurde.

**So lässt sich die App trotzdem sicher installieren:**
1. Klicke im blauen Fenster auf **Weitere Informationen** (More info).
2. Klicke anschließend auf den Button **Trotzdem ausführen** (Run anyway).
3. Da der gesamte Quellcode hier auf GitHub transparent einsehbar ist, kannst du der Ausführung absolut vertrauen.

---

## 🚀 Erste Schritte (First Use)

### 1. Vorbereitung des Simulator-Ordners
Für die erste Nutzung wird ein Standard-Simulator benötigt. Dies ist typischerweise der Omron Standard-Ordner: `C:\Users\GB89\Documents\OMRON FZ`.
Dieser Ordner sollte standardmäßig folgende Unterordner enthalten:
* `MED-Package`
* `RAMDisk`
* `SettingData`
* `USBDisk`

### 2. Pfade in den Einstellungen festlegen
Öffne die App-Einstellungen (Settings) und konfiguriere deine gewünschten Verzeichnisse:
* **Simulator Path:** Der Pfad zum aktuell aktiven Simulator (z. B. `C:\Users\GB89\Documents\OMRON FZ`).
* **Simulators Path:** Der Speicherort für deine verschiedenen Simulator-Versionen (z. B. `C:\Users\GB89\Documents\OMRON_FZ_Simulators`).
* **Simulators Backup Path:** Zielpfad für deine Simulator-Backups (z. B. `B:\OMRON_FZ_Simulator_Backups`).
* **USB Disk Path:** Speicherort für deine separaten USB-Disks (z. B. `C:\Users\GB89\Documents\OMRON_FZ_USBDisks`).
* **USB Disk Backup Path:** Zielpfad für deine USB-Disk-Backups (z. B. `B:\OMRON_FZ_Simulator_USBDiskBackups`).

### 3. Verwaltungsmodus wählen (Types of administration)
Es stehen zwei Modi zur Verfügung (Hinweis: Diese sind nicht zu 100 % miteinander kompatibel!):
* **Modus 0: Simulator and USBDisk is same.** Die USB-Disk wird immer fest an den Simulator gekoppelt. Beim Laden eines Simulators wird automatisch die gleichnamige USB-Disk gesucht und mitgeladen.
* **Modus 1: One USBDisk can use for more once Simulator and you can load Items separately.** Um Speicherplatz und Ladezeiten zu sparen, können Simulator und USB-Disk getrennt voneinander über den Button "Load Item" geladen werden. Soll ein Simulator zusammen mit seiner verknüpften USB-Disk geladen werden, nutzt du einfach den Button "Load". Auch Backups lassen sich so einzeln durchführen.

### 4. Erste Elemente anlegen
Zu Beginn haben aktive Elemente noch keinen Namen im System. Vergib einfach einen Namen und klicke auf Speichern (Save), um deinen ersten Eintrag in der Liste zu generieren.

---

## ❓ FAQ & Troubleshooting

**Wie erstelle ich einen neuen Simulator oder eine neue USB-Disk?** Benenne einfach den aktuell geladenen Simulator oder die USB-Disk um und klicke auf den entsprechenden Speichern-Button. Bestehende Dateien werden nur überschrieben, wenn sie bereits im Ziel existieren.

**Woher kennt die App die Namen der Simulators und USB-Disks?** * Die App erstellt im Simulator-Ordner eine entsprechende `*.gb`-Datei, die den Namen der verknüpften USB-Disk enthält.
* Im USB-Disk-Ordner wird ebenfalls eine `*.gb`-Datei angelegt, diese bleibt jedoch leer.

> ⚠️ **Achtung:** Diese `*.gb`-Dateien dürfen niemals gelöscht und möglichst nicht manuell verändert werden!

---

## 🐛 Bug Reports & Feature Requests

Du hast einen Fehler gefunden oder eine tolle Idee für ein neues Feature?  
Der beste Weg ist, direkt hier auf GitHub unter dem Reiter **Issues** ein neues Issue zu eröffnen. Alternativ kannst du mich auch gerne direkt kontaktieren!

---

## 📄 Lizenz & Autor

* **Entwicklung & Design:** Designed and Created by **Georg Black**.
* **Lizenz:** Dieses Projekt ist Open-Source und unter der **GNU General Public License v3.0 (GPLv3)** veröffentlicht. Du darfst die Software frei nutzen, modifizieren und weiterverbreiten, solange alle Änderungen unter derselben Open-Source-Lizenz bleiben. Weitere Details findest du in der `LICENSE`-Datei in diesem Repository.
