# Futtatás

### Alapok

A csapatban mindenki Windows-on dolgozik. CMAKE és GCC telepítése és CLion használatával készült.

### Használat

Build CMake-el, akár CLion-on belül.

### Futtatás
Paraméternek bemeneti fájl. Ha nincs második paraméter, akkor csak validál.

`./parser_demo.exe ../caff_files/1.caff`

Második paraméternek, kimenet megadásával.

`./parser_demo.exe ../caff_files/1.caff output.gif`

Fontos, hogy az elérési útban ne legyen "space", különben nem működik.

# Működés

A parser kinyer bizonyos információkat a CAFF fájlból.
- Szerző
- Kreálás ideje.
- Felbontás

Megnézi, hogy a leírásnak megfelelő-e a CAFF.
Ellenőrzi, hogy a CIFF frame-ek mérete megegyzeik. 


### Külső könyvtár

A gif-ek generálásához (kis módosításokkal túlindexelés ellen), az alábbi könyvtárt használtuk:

https://github.com/charlietangora/gif-h

# Egyéb védelem

Használjuk a `-fstack-protector-all` g++ kapcsolót a C++ fordításhoz (buffer owerflow ellen segít védekezni).

Ha ez miatt lassan fordul, használja a  `-fstack-protector` kapcsolót helyette (ez csak sérülékenynek titulált metódusokat védi).

# Gif

Elkészült gif-ek találhatók a caff-ok mellett.

