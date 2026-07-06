# Kiosk Simulator – 3D-Shop-Simulator (Unity, C#)

Ein vollständig spielbarer 3D-Kiosk-Simulator als MVP mit erweiterbarer Vollversionsstruktur.
Alle Assets sind prozedural generierte Platzhalter (Primitives, generierte Texturen, Sinuston-Sounds).
Alle Produkte, Tabakwaren, Lotto-Scheine und Marken sind **komplett fiktiv**. Kein echtes Geld, kein echtes Glücksspiel, offline spielbar.

---

## A. Projektübersicht

Der Spieler leitet einen kleinen Kiosk in First-Person-Ansicht:

* Begehbarer 3D-Kiosk mit Regalen, Kühlschrank, Theke, Kasse, Lager, Paketregal, Lotto-Terminal und Tabakwaren-Schrank
* Kunden-KI: Kunden betreten den Laden, wählen Produkte nach Nachfragewert, stellen sich an, bezahlen bar oder mit Karte und verlassen den Laden. Geduld sinkt beim Warten, unzufriedene Kunden senken den Ruf.
* Kassensystem mit Scan-Mechanik, Bar-/Kartenzahlung und Altersprüfung für fiktive Tabakwaren
* Warenbestellung über das Tablet (Tab), Lieferungen kommen nach Timer als Kartons an und werden ins Lager ausgepackt
* Paketannahme (Kunden bringen Pakete) und Paketabholung (Kunden nennen Code)
* Fiktives Lotto-Terminal mit Provision und kleinen Zufallsgewinnen
* Wirtschaftssystem: Startkapital 2.500 €, tägliche Miete und Stromkosten, Umsatz, Gewinn, Strafen, Tagesabschluss, Wochenstatistik
* Fortschritt: XP, Level, Freischaltungen (Level 1–10), 25 Shop-Upgrades mit sichtbaren Effekten im Laden
* Tag-/Nacht-Zyklus mit Öffnungszeiten (8:00–20:00, ein Tag = 12 Echtzeitminuten)
* JSON-Speichersystem (Speichern/Laden über Pausenmenü und Hauptmenü)

**Architektur-Kernidee:** Die Szenen enthalten jeweils nur ein Bootstrapper-GameObject.
`SceneBootstrapper` (Spiel) und `MainMenuBootstrapper` (Menü) erzeugen beim Start den kompletten
Raum, alle Möbel, Manager, den Spieler, die UI und das EventSystem zur Laufzeit über
`ProceduralAssetGenerator`. Dadurch ist das Projekt sofort spielbar, ohne dass Prefabs oder
Szeneninhalte manuell gepflegt werden müssen – und hochwertige Assets können später gezielt
die Platzhalter-Erzeugung ersetzen.

## B. Installations- und Setup-Anleitung

1. Repository klonen oder herunterladen.
2. Unity Hub öffnen → **Add** → Projektordner auswählen.
3. Projekt mit **Unity 2022.3 LTS** öffnen (erster Import dauert einen Moment).
4. Szene `Assets/Scenes/MainMenu.unity` öffnen (oder direkt `KioskGame.unity`).
5. **Play** drücken → „Neues Spiel“ → der Kiosk wird automatisch generiert und das Spiel läuft.

Falls die minimal gehaltenen Szenendateien in einer anderen Unity-Version nicht laden:
Menü **Kiosk → Szenen neu generieren** ausführen (siehe Abschnitt E), danach erneut Play drücken.

**Steuerung:**

| Taste | Aktion |
|---|---|
| WASD | Bewegung |
| Maus | Umsehen |
| E | Interagieren (Kasse, Regal, Karton, Lager, Paketregal, Lotto-Terminal) |
| Tab | Tablet öffnen/schließen (Bestellung, Upgrades, Lager, Pakete) |
| Escape | Pausenmenü / Fenster schließen |

## C. Unity-Version

* **Unity 2022.3.20f1 (LTS)** – jede 2022.3.x-Version funktioniert.
* Keine externen Pakete nötig: nur `com.unity.ugui` und Unity-Core-Module (siehe `Packages/manifest.json`).
* Legacy Input Manager und uGUI (kein TextMeshPro, kein neues Input System, kein NavMesh-Paket).

## D. Komplette Ordnerstruktur

```
Assets/
  Scenes/
    MainMenu.unity          (nur MainMenuBootstrapper – Menü wird generiert)
    KioskGame.unity         (nur SceneBootstrapper – Spiel wird generiert)
  Scripts/
    Core/        GameManager, DayNightCycle, SceneBootstrapper, MainMenuBootstrapper,
                 DefaultGameData, UnlockManager, AutomationController, ProceduralAssetGenerator
    Player/      PlayerController, PlayerInteractor
    Interaction/ Interactable
    Products/    ProductData, ProductInstance
    Inventory/   InventoryManager, StorageManager
    Shelves/     Shelf, ShelfSlot
    Checkout/    CheckoutCounter, CashRegister, PaymentSystem
    Customers/   CustomerAI, CustomerSpawner, CustomerQueue, CustomerNeeds
    Orders/      OrderSystem, SupplierData
    Delivery/    DeliverySystem, DeliveryBox
    Packages/    PackageSystem, PackageItem
    Lotto/       LottoTerminal, LottoTicketData
    AgeRestricted/ AgeCheckSystem
    Economy/     EconomyManager, ReputationManager
    Upgrades/    UpgradeData, UpgradeManager
    UI/          UIManager, TabletUI, CheckoutUI, OrderUI, UpgradeUI,
                 DaySummaryUI, MainMenuUI, PauseMenuUI
    SaveSystem/  SaveLoadSystem
    Audio/       AudioManager
    Editor/      KioskEditorSetup
  Prefabs/       Player/ Customers/ Products/ Shelves/ Checkout/ Delivery/ Packages/ UI/
                 (leer – alles wird zur Laufzeit generiert; hier später echte Prefabs ablegen)
  ScriptableObjects/ Products/ Upgrades/ Customers/
                 (per Editor-Tool „Kiosk → Daten als ScriptableObjects exportieren“ befüllbar)
  Materials/  Textures/  Audio/  Fonts/  Resources/
                 (per Editor-Tool befüllbar; Ablageort für spätere echte Assets)
Packages/manifest.json
ProjectSettings/ (Unity-Version, Build-Szenenliste)
```

## E. Automatischer Setup-Editor-Code

`Assets/Scripts/Editor/KioskEditorSetup.cs` fügt das Menü **Kiosk** in Unity hinzu:

* **Kiosk → Szenen neu generieren** – erzeugt `MainMenu.unity` und `KioskGame.unity` neu
  (Fallback, falls die Szenendateien nicht laden) und trägt sie in die Build Settings ein.
* **Kiosk → Daten als ScriptableObjects exportieren** – exportiert alle 46 Produkte,
  25 Upgrades, Lieferanten und Lotto-Scheine als `.asset`-Dateien nach `Assets/ScriptableObjects/`.
* **Kiosk → Materialien und Icons exportieren** – schreibt alle prozeduralen Materialien
  (Material_Boden, Material_Wand, Material_Regal, Material_Kasse, Material_Kuehlschrank,
  Material_Produkt_*, Material_Paket, Material_LottoTerminal, Material_Kunde) und Icon-Texturen
  (Texture_Icon_Geld, _Bestand, _Bestellung, _Upgrade, _Paket, _Lotto, _Alter)
  nach `Assets/Materials/` bzw. `Assets/Textures/`.
* **Kiosk → Spielstand löschen** – entfernt die JSON-Speicherdatei.

Für das reine Spielen ist **kein** Editor-Schritt nötig – alles entsteht zur Laufzeit.

## F. Alle C#-Scripts

Alle 45 Pflicht-Scripts sind vollständig implementiert (siehe Ordnerstruktur in D).
`CheckoutUI` deckt die Kassen-UI ab, `SceneBootstrapper` erzeugt Szene und UI,
`KioskEditorSetup` ist das Editor-Setup-Tool. Zusätzlich: `MainMenuBootstrapper`,
`DefaultGameData` und `AutomationController` (Mitarbeiter-Kasse, Self-Checkout, Auto-Auffüllung).

## G. Produktdaten

`Assets/Scripts/Core/DefaultGameData.cs` enthält die Startdatenbank mit **46 fiktiven Produkten**
in allen geforderten Kategorien (Wasser, Softdrinks, Energydrinks, Kaffee, Chips, Schokolade,
Bonbons, Kaugummi, Eis, Zeitungen, Magazine, fiktive Tabakwaren, fiktive E-Zigaretten, Feuerzeuge,
fiktive Lotto-Scheine, fiktive Rubbellose, Prepaid-Karten, Handy-Zubehör, Batterien, Hygieneartikel,
Paketmarken, Geschenkartikel, Saisonartikel). Jedes Produkt hat ID, Name, Kategorie, Einkaufs-/
Verkaufspreis, Lager-/Regalgröße, Nachfragewert, Freischaltlevel, Altersbeschränkung, Lotto-Flag,
Modelltyp, Material und Icon. Export als ScriptableObjects über das Kiosk-Menü (siehe E).

## H. Upgrade-Daten

Ebenfalls in `DefaultGameData.cs`: **25 Upgrades** (Regal Stufe 2/3, Zweites Regal,
Getränke-/Premium-Kühlschrank, Zweite/Schnellere Kasse, Kartenzahlung, Self-Checkout,
Größeres Lager, Paketregal Stufe 2, Paketstation, Lotto-Terminal Stufe 1/2, Tabakwaren-Schrank,
Sicherheitskamera, Diebstahlschutz, Bessere Beleuchtung, Klimaanlage, Kaffeemaschine,
Außenwerbung, Schaufensterdeko, Mitarbeiter-Kasse, Automatische Regalauffüllung,
Lieferantenrabatt) mit ID, Name, Beschreibung, Kosten, Freischaltlevel, Voraussetzung,
Effekt (`UpgradeEffect`-Enum) und visuellem Effekt im Laden (`SceneBootstrapper.ApplyUpgradeVisual`).

## I. Szene-Bootstrapper

`SceneBootstrapper.cs` (Execution Order -100) baut beim Start:
Boden, Wände, Decke, Eingang, Beleuchtung, Kamera/Spieler mit Spawnpunkt, alle Manager,
Regale, Kühlschrank (per Upgrade), Theke mit Kasse, Lager, Paketregal, Lotto-Terminal,
Tabakwaren-Schrank, Kunden-Wegpunkte/Spawnpunkte und Start-Warenbestand.
Kunden nutzen einfache Wegpunktnavigation (kein NavMesh nötig).

## J. UI-Bootstrapper

`UIManager.cs` erzeugt Canvas + EventSystem und die komplette HUD/Fenster-UI zur Laufzeit:
Geld, Uhrzeit, Tag, Ruf, XP/Level, Interaktionshinweis, Kundenwunsch, Kassenfenster mit
Scan-Liste und Altersprüfungs-Panel, Tablet mit Bestell-, Upgrade-, Lager- und Paket-Tabs,
Lotto-Terminal-UI, Tagesabschluss, Toast-Meldungen, Pausenmenü. `MainMenuUI` baut das Hauptmenü.

## K. Testanleitung

1. `MainMenu.unity` öffnen, Play drücken → „Neues Spiel“.
2. Spieler steht hinter der Theke, Produkte liegen in den Regalen.
3. Warten: Kunden betreten den Laden, nehmen Produkte, stellen sich an der Kasse an.
4. Zur Kasse gehen, **E** drücken → Kassenfenster: Artikel scannen, „Bar“ oder „Karte“ → Geld steigt.
5. Bei Tabak-Produkten erscheint die Altersprüfung: Ausweis prüfen, verkaufen oder ablehnen.
6. **Tab** → Bestellmenü: Produkte bestellen → Lieferkarton erscheint nach Timer am Eingang →
   mit **E** auspacken → Ware liegt im Lager → am Regal mit **E** auffüllen.
7. **Tab** → Upgrades: z. B. „Getränke-Kühlschrank“ kaufen → Kühlschrank erscheint im Laden.
8. Paketkunde bedienen (Paket scannen), Abholkunde bedienen (Code vergleichen, Paket ausgeben).
9. Lotto-Kunde bedienen (Schein verkaufen, Provision erhalten) – ab Level 5 mit Terminal-Upgrade.
10. Um 20:00 schließt der Laden → Tagesabschluss mit Umsatz/Kosten/Gewinn → nächster Tag.
11. **Escape** → „Speichern“; Spiel beenden; erneut starten → „Spiel laden“ → Stand ist wiederhergestellt
    (Speicherdatei: `%userprofile%/AppData/LocalLow/DefaultCompany/kiosk-simulator/kiosk_save.json`
    bzw. `Application.persistentDataPath`).

## L. Bekannte Einschränkungen

* Alle Modelle sind Primitives, Texturen prozedural, Sounds Sinuston-Platzhalter.
* Kundennavigation ist einfache Wegpunkt-Steuerung ohne Hindernisvermeidung untereinander.
* Wechselgeld-Mechanik ist vereinfacht (automatisch korrekt).
* Kein Multiplayer, keine Mitarbeiter-Charaktermodelle (Mitarbeiter-Kasse wirkt als Automatisierung).
* Speicherstand speichert einen Slot; Regalzuordnung setzt unveränderte Regal-Reihenfolge voraus.
* Szenendateien sind minimal gehalten – bei Problemen „Kiosk → Szenen neu generieren“ nutzen.

## M. Erweiterungsschritte

1. Echte 3D-Modelle/Texturen in `Assets/Prefabs` bzw. `Assets/Materials` ablegen und in
   `ProceduralAssetGenerator.CreateProductModel/CreateCustomerModel/GetMaterial` laden.
2. Echte Audiodateien in `Assets/Audio` ablegen und im `AudioManager` den `SoundId`s zuweisen.
3. TextMeshPro + eigenes UI-Design statt generierter uGUI-Elemente.
4. NavMesh-Navigation (com.unity.ai.navigation) statt Wegpunkten.
5. Mehr Produkte/Upgrades: einfach `DefaultGameData` erweitern oder ScriptableObjects anlegen.
6. Mitarbeiter mit Modellen und Aufgaben-KI, Ladendiebe, Events (Stoßzeiten, Feiertage).
7. Mehrere Filialen, größere Ladenlayouts, Innenausbau-Editor.
8. Steam-/Cloud-Save, mehrere Speicherslots, Statistik-Graphen.
