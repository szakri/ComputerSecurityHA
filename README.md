# ComputerSecurityHA
## Általános leírás
A félév során egy open source alkalmazást kell elkészíteni csapatokban. A projekt célja a tanult módszerek alkalmazása a gyakorlatban, egy biztonságkritikus szoftver teljes tervezési, fejlesztési, és tesztelési folyamatának az elvégzése. Egy olyan online áruházat kell készíteni, amiben egyedi formátumú animált képeket lehet böngészni. A szoftvernek a CAFF (CrySyS Animated File Format) formátumot kell támogatnia. A teljes rendszer kell, hogy tartalmazzon egy HTTP(S) protokollon elérhető távoli szolgáltatást, valamint az ahhoz tartozó webes vagy mobil klienst. 
### Felhasznált formátumok:
- [CAFF fájlformátum](https://www.crysys.hu/downloads/vihima06/2020/CAFF.txt)
    - tömörítés nélküli animációformátum
    - CIFF képek tárolására alkalmas
    - az animációhoz tartozó metaadatokat tárolja
    - [Példa fájlok a teszteléshez](https://edu.vik.bme.hu/mod/resource/view.php?id=24410)
- [CIFF fájlformátum](https://www.crysys.hu/downloads/vihima06/2020/CIFF.txt)
    - tömörítés nélküli képformátum
    - pixel informácikat tartalmaz
    - a képhez tartozó metaadatokat tárolja
### Követelmények:
- Funkcionalitás
    - felhasználóknak kell tudni regisztrálni és belépni
    - felhasználóknak kell tudni CAFF fájlt feltölteni, letölteni, keresni
    - felhasználóknak kell tudni CAFF fájlhoz megjegyzést hozzáfűzni
    - a rendszerben legyen adminisztrátor felhasználó, aki tud adatokat módosítani, törölni
- Szerver oldali funkciókövetelmények
    - CAFF feldolgozási képesség
    - teljesítménymegfontolásokból C/C++ nyelven kell implementálni
    - feladat: a CAFF fájlból egy előnézet generálása a webshopban megjelenítéshez
- Kliens oldali követelmények
    - vagy egy webes vagy iOS vagy Android implementáció
