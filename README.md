UIST-WS1415
===========


Ex 4
===========
- Die letzten 5 Werte des Durchmessers werden in einem Array gespeichert
- Aus diesen Werten wird der Mittelwert berechnet
- Es wird geprüft, ob der letzte gemessene Wert eine große Abweichung vom akutell gemessen Wert hat
  - Bei hohe Abweichung wird der neue Wert zum alten hin verschoben um Ausschläge abzudämpfen
- Ein Simpler Hochpass filter schließt jetzt Werte aus, die eine sehr kleine distanz haben, da in diesem Berech zuviele Houghcircles von OpenCV erkannt werden

Um eine Logdatei zu erstellen muss die .exe mit dem Parameter "> dateiname.log" (ohne ") über die Kommandozeile ausgeführt werden.


