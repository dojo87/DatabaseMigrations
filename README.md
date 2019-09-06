# Migracja Bazodanowa

Pomiń wstępniak >>

## Emigracja nie taka fajna

Jak w świecie, tak w wielu projektach IT temat wszelakich migracji, emigracji i imigracji może rozpalić.
Ze łzą w oku wspominam projekt do którego dołączyłem (wyemigrowałem) już w jego trakcie, ale jeszcze w fazie przed premierą. Nowa praca, nowe wyzwania, yeah!
Zdaje się, że w 09/2012 został zaprezentowany Entity Framework 4 z podejściem Code First, a w tym samym miesiącu baza danych projektu mogła już mieć z 100MB + 20MB CMS.
Już pierwsze zaciągnięcie z SVN'a nie zapowiadało dobrego startu. Wszystkie skompilowane binaria C# zaciągały się również. Wisienką na torcie były jednak dwa rozpaśne pliki \*.bak.

## Czy leci z nami pilot

Już wkrótce czekała mnie pierwsza zmiana na bazie danych.

1. Zaciągnąłem świeży \*.bak z SVN'a
2. Wykonałem restore na moim lokalnym MSSQL. Ajć, zapomniałem ustawić opcji, żeby wyczyścił starą bazę do cna, w przeciwnym wypadku mieliśmy jakieś dziwne problemy... ale nie pamiętam już na szczęście o co chodziło. No to jeszcze raz restore. Yes, udało się.
3. Szczęście, że zespół był w jednym pomieszczeniu, więc uprzejmie informowałem, że dokonuję zmiany na bazie danych (oj takie tam dodawanie kolumny). Tak naprawdę przebąkiwałem już o tej zmianie trochę wcześniej, żeby nikt inny nie zabierał się za podobne wyczyny.
4. Dodałem kolumnę, na szybko sprawdziłem czy czegoś nie zapomniałem.
5. Super, Backup robię zamiast starego \*.bak i jazda na SVN z 100MB'owym plikiem.
6. Uprzemie informowałem, że skończyłem.

Wrzucenie tego na serwer testowy było nieco krótsze. Kopiowanie \*.bak na dysk sieciowy i informacja do technicznego managera u klienta, że może sobie zaaplikować. To nic, że dane się resetowały - to w końcu jeszcze testy :)

Co robisz jak trafiasz do takiego projektu? Nie, nie uciekasz. Mierzysz się z Goliatem!

## 6 wyzwań migracji

Najprostrzy scenariusz zakłada, że dane w trakcie działania aplikacji się nie zmieniają i są zawsze dostarczane z nową wersją jako statyczny zasób. A no i trzeba jeszcze założyć, że zespół składa się z jednego programisty. W przeciwnym wypadku i tak trzeba znaleźć sposób na dzielenie się tą samą bazą bez zaburzenia pracy innych. W takim scenariuszu jednak i tak wybrany sposób aktualizacji takiej bazy może być znacząco prostrzy od baz "żyjących swoim życiem".
To jest znacząca różnica między kodem programu, który zwykle dostarczymy kopiując kilka plików, a danymi, które mogą się zmieniać i mają swoją autonomię. Dodając do tego przykład ze wstępu, widzimy jakie wyzwania nas czekają:

1. **Źródło Prawdy** Musisz wybrać źródło prawdy (jak i w życiu) co do modelu bazy; gdzie będziesz ją (prawdę) trzymać? Czy baza danych czy kod programu definiują jej strukturę? _Database First_ czy _Code First_? A może _Model First_? Nową wersję przechowujemy jako stan bazy czy listę zmian?
2. **Aktualizacja Struktur i Danych** Musisz zabezpieczyć możliwość aktualizacji bazy produkcyjnej bez zniszczenia danych:
   a. dodawanie/usuwanie/zmienianie kolumny w bazach relacyjnych;
   b. aktualizacja struktur (np. w NoSQL)
   c. aktualizacja danych lub synchronizowanie danych (np. w przypadku, gdy mamy tabelę z wartościami konfiguracyjnymi aplikacji, która może być zmieniana w panelu admina, ale jednocześnie chcemy zapewnić, że domyślne dane zawsze tam będą).
3. **Automatyzacja** Powinieneś uwzględnić możliwość automatycznego wykonania takiej aktualizacji. Jak wykrywasz różnice? Jak te różnice zaaplikować na bazę?
4. **Współdzielenie Kodu i Rozwiązywanie Konfliktów** Jak rozwiązujesz konflikty w repozytorium kodu? Ups, Nie wspomniałem? TAK, model bazy powinien być częścią kodu.
5. **Dywersyfikacja Środowiska** Czy potrzebujesz zróżnicować dane początkowe ze względu na środowisko (Dev, Test, QA, Produkcja)? Może lista wymaganych użytkowników musi być różna? Może na Dev/Test chcesz nawrzucać trochę śmieci testowych? Więc jak to zrobisz?
6. **Synchronizacja z warstwą dostępu do danych** Jak łatwo przeniesiesz zmiany na bazie do kodu aplikacji?

Powyższe wyzwania uniwersalne bez względu na język programowania czy bazę. No może poza wyjątkiem użycia Oracle'a. Wtedy będzie trzeba dołożyć do listy ;)
W kolejnych rozdziałach zajmę się wybranymi rozwiązaniami tego zagadnienia wykorzystującymi podejście _Database First_ i _Code First_ co odzwierciedla **Źródło Prawdy** - to gdzie przechowywana jest wiedza na temat stanu bazy. Odpowiadając sobie na pytanie "jakie podejście zastosujesz jeśli chodzi o bazę danych", zwykle programista pomyśli "Database First" albo "Code First" albo "Model First". W drugiej kolejności przyjdą odpowiedzi pytania 2-6. To pokazuje, że jest to jednak najistotniejsza sprawa.
W artykule pominę _Model First_ z którym osobiście najmniej miałem do czynienia. Warto jednak mieć go z tyłu głowy jako dobry start dla projektu, gdy trzeba modelować strukturę danych ściśle według wskazówek eksperta domeny, który nie rozumie SQL'a czy C#. Dla programisty jednak zwykle _Model First_ wprowadza dodatkową abstrakcję, która nie ma wartości dodanej jeśli chodzi o działanie aplikacji. Na pewno jednak pozwala lepiej zobrazować stan bazy.

### Źródło Prawdy - stan czy tranzycje?

Niezależnie od wybranego podejście nie jest dobrze, aby strukturę bazy danych trzymać w samej instancji bazy albo jakimkolwiek binarnym tworze. Powinna ona sprowadzać się do kodu utrzymanego w kontroli wersji (to raz) i pozwalać na łatwą edycję i rozwiązywanie konfliktów. Mając jednak kod możemy trzymać **stan** struktury bazy (zbiór tabel, widoczków itd.) i/lub listę zmian powstałych w trakcie rozwoju oprogramowania - **tranzycji**.  
Nawet jeśli w kodzie będziemy przechowywać sam **stan** to musimy umieć uzyskać tranzycje, które będą użyte do zaktualizowania baz danych na kolejnych środowiskach - może to jednak być zautomatyzowane (choć lepiej, żeby generowanie takiego skryptu _musiało_ być automatyczne).

## Database First

Podejście database first zakłada, że źródło prawdy jest zdefiniowane w języku bazy danych. Ona powstaje pierwsza i determinuje model i kod programu.
W przypadku _Database First_ **stanem** będzie zbiór skryptów opisujących pełne obiekty bazy danych np. zupełna definicja tabeli:

```sql
CREATE TABLE dbo.MyTableState
(
	Id bigint NOT NULL,
	Version int NOT NULL,
	ColumnAddedInVersion2 varchar(200) NOT NULL
)
```

natomiast **tranzycją** skrypt, który zawiera wszystkie zmiany na bazie dla kolejnej jej wersji:

```sql
ALTER TABLE dbo.MyTableState
ADD COLUMN ColumnAddedInVersion2 VARCHAR(200) NOT NULL
```

Możemy również połączyć jedno i drugie aby w danej chwili mieć pełny obraz bazy danych w kodzie. Zaimplementujemy rozwiązanie, następnie obronimy go względem "szóstki".

### Narzędzia

1. [SqlProject dla Visual Studio](https://docs.microsoft.com/en-us/sql/ssdt/download-sql-server-data-tools-ssdt) do zarządzania skryptami
2. [SqlPackage.exe](https://docs.microsoft.com/en-us/sql/tools/sqlpackage-download) do automatyzacji
3. [DbUp](https://dbup.readthedocs.io/en/latest/) do automatyzacji wersji wzbogaconej o tranzycje
4. [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) do synchronizacji kodu (użyjemy v2.x.x)

### Implementacja

Nową solucję stworzymy zaczynając od projektu _SQL Server Database Project_.
Produktem tego projektu jest binarny plik DACPAC zawierający `schema` i dane pochodzące ze skryptów. Pozwala on narzędziu SqlPackage na porównanie DACPAC z docelową bazą danych i automatyczne wygenerowanie skryptów tranzycji oraz zastosowanie ich na tej bazie.
Stworzymy prostą bazę zawierającą tabelę `Topics` z linkami do artykułów tematycznych oraz `Tags` do oznaczania tematów odpowiednimi etykietami. Pomiędzy nimi zachodzi relacja wiele do wielu - stąd tabela pośrednicząca TopicTags. Na koniec dodamy skrypt do wypełnienia tabel danymi testowymi - TestData.sql. W nawiasie podaję _Build Action_ jaki musimy ustawić we właściwościach pliku (PPM na pliku / Properties).

```
|- Schema
   |- Tags.sql (Build)
   |- Topics.sql (Build)
   |- TopicTags.sql (Build)
|- PostScripts
   |-TestData.sql (PostDeploy)
```

W _Solution Explorer_ na tym projekcie mamy opcję _Publish_. Wybierając ją możemy zdefiniować profil publikowania bazy danych.
Ustawmy:

1. _Connection String_ połączenia do bazy danych - ja użyje lokalnej bazy dla _Debug_
   `Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=False;TrustServerCertificate=False`
2. Nazwę bazy danych na _TopicalTags_
3. Nazwę skryptu _TopicalTags.sql_

Resztę pozostawiamy domyślnie, natomiast profil chcemy zapisać - Save Profile As...
![Publish Profile](https://photos.app.goo.gl/GuXrJnrpAmrHkhUx8)

## Code First (Entity Framework)

## Usprawiedliwenie

Pora rozgrzeszyć wspomniany we wstępie projekt. Chociaż moje wejście do zespołu nie należało do najłatwiejszych, bo byłem najmłodszy. Co tu taki podrostek będzie przewracał ustanowiony porządek? Niemniej wydaje mi się, że pierwszym problemem było nadanie całemu projektowi charakteru prototypu, który przegapił moment stania się faktycznym przedsięwzięciem a drugim swego rodzaju bezkrólewie, bo również lider był już jedną nogą i czterema palcami w innym projekcie. W pewnym momencie ktoś z góry uznał, że wchodzimy na żywo i nikt się tego nie spodziewał. Na szczęście z teamem ostatecznie doszliśmy do dobrych relacji i razem udało się rozwiązać kwestię bazy. W tym przypadku zastosowaliśmy dedykowane rozwiązanie Database First przez Tranzycje.
