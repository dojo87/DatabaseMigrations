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
6. **Synchronizacja z aplikacją - warstwą dostępu do danych** Jak łatwo przeniesiesz zmiany na bazie do kodu aplikacji?

Powyższe wyzwania uniwersalne bez względu na język programowania czy bazę. No może poza wyjątkiem użycia Oracle'a. Wtedy będzie trzeba dołożyć do listy ;)
W kolejnych rozdziałach zajmę się wybranymi rozwiązaniami tego zagadnienia wykorzystującymi podejście _Database First_ i _Code First_ co odzwierciedla **Źródło Prawdy** - to gdzie przechowywana jest wiedza na temat stanu bazy. Odpowiadając sobie na pytanie "jakie podejście zastosujesz jeśli chodzi o bazę danych", zwykle programista pomyśli "Database First" albo "Code First" albo "Model First". W drugiej kolejności przyjdą odpowiedzi pytania 2-6. To pokazuje, że jest to jednak najistotniejsza sprawa.
W artykule pominę _Model First_ z którym osobiście najmniej miałem do czynienia. Warto jednak mieć go z tyłu głowy jako dobry start dla projektu, gdy trzeba modelować strukturę danych ściśle według wskazówek eksperta domeny, który nie rozumie SQL'a czy C#. Dla programisty jednak zwykle _Model First_ wprowadza dodatkową abstrakcję, która nie ma wartości dodanej jeśli chodzi o działanie aplikacji. Na pewno jednak pozwala lepiej zobrazować stan bazy.

### Źródło Prawdy - stan czy tranzycje?

Niezależnie od wybranego podejście nie jest dobrze, aby strukturę bazy danych trzymać w samej instancji bazy albo jakimkolwiek binarnym tworze. Powinna ona sprowadzać się do kodu utrzymanego w kontroli wersji (to raz) i pozwalać na łatwą edycję i rozwiązywanie konfliktów. Mając jednak kod możemy trzymać **stan** struktury bazy (zbiór tabel, widoczków itd.) i/lub listę zmian powstałych w trakcie rozwoju oprogramowania - **tranzycji**.  
Nawet jeśli w kodzie będziemy przechowywać sam **stan** to musimy umieć uzyskać tranzycje, które będą użyte do zaktualizowania baz danych na kolejnych środowiskach - może to jednak być zautomatyzowane (choć lepiej, żeby generowanie takiego skryptu _musiało_ być automatyczne).

## Database First ze stanem

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

### Podstawowa Implementacja - Aktualizacja i Automatyzacja

Nową solucję stworzymy zaczynając od projektu _SQL Server Database Project_.
Produktem tego projektu jest binarny plik DACPAC zawierający `schema` i dane pochodzące ze skryptów. Pozwala on narzędziu SqlPackage na porównanie DACPAC z docelową bazą danych i automatyczne wygenerowanie skryptów tranzycji oraz zastosowanie ich na tej bazie.
Stworzymy prostą bazę zawierającą tabelę `Topic` z linkami do artykułów tematycznych oraz `Tag` do oznaczania tematów odpowiednimi etykietami. Pomiędzy nimi zachodzi relacja wiele do wielu - stąd tabela pośrednicząca TopicTags. Na koniec dodamy skrypt do wypełnienia tabel danymi testowymi - `TagConfiguration.sql` z listą tagów oraz `TestData.sql` z testową listą tematów. Będą one korzystać z opcji "Post Deploy", co spowoduje, że wykonają się po tym jak wszystkie tabelki będą gotowe. Z uwagi na to jednak, że możemy mieć tylko jeden skrypt "Post" lub "Pre" w projekcie, to połączymy je dodatkowym plikiem `PostScript.sql`
W nawiasie podaję _Build Action_ jaki musimy ustawić we właściwościach pliku (PPM na pliku / Properties).

```
|- Schema
   |- Tag.sql (Build)
   |- Topic.sql (Build)
   |- TopicTags.sql (Build)
|- PostScripts
   |- PostScript.sql (Post Deploy)
   |- TagConfiguration.sql (None)
   |- TestData.sql (None)
```

A tak wyglądają skrypty:

```sql
-- Tag.sql
CREATE TABLE [dbo].[Tag]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Name] VARCHAR(255)
)
```

```sql
-- Topic.sql
CREATE TABLE [dbo].[Topic]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Title] VARCHAR (2000) NOT NULL,
	[Url] VARCHAR (2000) NOT NULL
)
```

```sql
-- TopicTags.sql
CREATE TABLE [dbo].[TopicTags]
(
	[TopicId] INT NOT NULL,
	[TagId] INT NOT NULL,
	CONSTRAINT PK_TopicTags PRIMARY KEY (TopicId, TagId),
    CONSTRAINT [FK_TopicTag_ToTopic] FOREIGN KEY ([TopicId]) REFERENCES [Topic]([Id]),
    CONSTRAINT [FK_TopicTag_ToTag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id])
)
```

```sql
-- TestData.sql
IF NOT EXISTS (SELECT TOP 1 Id FROM Topic)
BEGIN
    PRINT 'Data seed - Inserting test [Topic]'
	INSERT INTO Topic (Id, Title, Url) VALUES (1, 'Origin of Life Problems for Naturalists','https://answersingenesis.org/origin-of-life/origin-of-life-problems-for-naturalists/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (1,1), (1,9), (1,10);

	INSERT INTO Topic (Id, Title, Url) VALUES (2, 'Power Plants','https://answersingenesis.org/biology/plants/power-plants/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (2,1), (2,4), (2,5), (2,6);

	INSERT INTO Topic (Id, Title, Url) VALUES (3, 'Evidence for a Young World','https://answersingenesis.org/astronomy/age-of-the-universe/evidence-for-a-young-world/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (3,1), (3,4), (3,7), (3,8);

	INSERT INTO Topic (Id, Title, Url) VALUES (4, 'Are Atheists Right? Is Faith the Absence of Reason/Evidence?','https://answersingenesis.org/christianity/are-atheists-right/');
	INSERT INTO TopicTags (TopicId, TagId) VALUES (4,1), (4,2), (4,3);

END;
```

```sql
-- TagConfiguration.sql

-- W przypadku Tagów zastosujemy podejście UPSERT.
-- Chcemy zawsze zadbać o to, że w bazie będzie te 10 tagów jednocześnie pozwalając, że w aplikacji może zachodzić jakaś administracja i dodawanie nowych tagów.

-- Cloning Tags into temp table
SELECT TOP 0 * INTO #TempTags FROM dbo.[Tag]

-- Seeding data
PRINT 'Data seed - Merging [Tags]'
INSERT INTO #TempTags ([Id], [Name]) VALUES
		(1, 'Answers'),
		(2, 'Worldview'),
		(3, 'Christianity'),
		(4, 'Science'),
		(5, 'Biology'),
		(6, 'Plants'),
		(7, 'Astronomy'),
		(8, 'Age of the Universe'),
		(9, 'Evolution'),
		(10, 'Origin of Life');

-- UPSERT
merge into dbo.[Tag] as Target
using #TempTags as Source
on Target.Id=Source.Id
when matched then
update set Target.[Name] = Source.[Name]
when not matched then
insert ([Id], [Name]) values (Source.[Id], Source.[Name]);

DROP TABLE #TempTags;
```

```sql
-- PostScript.sql
print "Running Post Deploy Scripts"
:r .\TagConfiguration.sql
:r .\TestData.sql

```

W _Solution Explorer_ na tym projekcie mamy opcję _Publish_. Wybierając ją możemy zdefiniować profil publikowania bazy danych.
Ustawmy:

1. _Connection String_ połączenia do bazy danych - ja użyje lokalnej bazy dla _Debug_
   `Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;`
2. Nazwę bazy danych na _TopicalTags_
3. Nazwę skryptu _TopicalTags.sql_

Resztę pozostawiamy domyślnie, natomiast profil chcemy zapisać - Save Profile As...
![Publish Profile](./Course/img/PublishProfile.PNG)

W tej chwili możemy już zaktualizować automatycznie naszą bazę - przez Publish, lub wygenerować skrypt, który aplikujemy na bazę (uwaga! jest on przygotowany do trybu działania przez SQLCMD więc użycie w Management Studio wymaga kilku zabiegów, które tutaj pominiemy).

Plik \*.publish.xml możemy również użyć w narzędziu _SqlPackage.exe_. Utworzymy sobie krótki skrypt DeployDebug.ps1:

```powershell
# Ścieżka Zależy od wersji
$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
&$sqlPackageExe /Action:Publish  /SourceFile:"./bin/Debug/TopicalTags.dacpac" /Profile:TopicalTags.publish.xml
```

Odpalenie skryptu już bez pytania aktualizuje nam bazę. Możemy jednak na wiele innych sposobów ustawić sobie parametry tego procesu np. w Continues Delivery - SqlPackage ma wiele parametrów https://docs.microsoft.com/en-us/sql/tools/sqlpackage.

### Rozwiązywanie Konfliktów

Taki zestaw skryptów bez problemu udostępniamy w naszym repozytorium kodu i rozwiązywanie konfliktów odbywa się jak w przypadku każdego innego pliku z kodem.

### Dywersyfikacja Środowiska

Dla środowiska lokalnego i deweloperskiego wrzucamy sobie podstawowe `Tag` i trochę testowych `Topic`. Na QA a już w szczególności na Produkcji nie chcemy tych danych. Przydadzą się jednak wciąż Tagi - załóżmy, że zawsze chcemy, aby w aplikacji było dostępnych naszych 10 tagów ze skryptu, natomiast administratorzy będą mogli dodawać więcej.

Skorzystamy z konfiguracji kompilacji, jakie możemy sobie ustawić w naszym projekcie. Mamy już `Debug` i `Release`, możemy również dodać kolejne typu `QA/Test/Dev`. Konfiguracje te są również łatwe do wybrania w różnych środowiskach automatyzujących proces kompilacji np. w TFS.
**No, ale jak sprawić, że dla Debug czy Dev będziemy mieli dane testowe a dla pozostałych tylko wymaganą konfigurację?**

Wykorzystamy Pre i Post Build Events:
![Build Events dla TopicalTags](./Course/img/BuildEvents.PNG)

**Co będzie robić tajemniczy skrypt SwitchToEnvironmentFile.ps1?**
TODO FILMIK

Jako parametr przyjmujemy plik "bazowy" Post lub Pre Deploy oraz wybraną konfigurację.
Śledząc wykonanie w Pre-Build:

```powershell
param (

    [Parameter(Position = 0, Mandatory = $true)]
    [string]
    $fileName,
    [Parameter(Position = 1, Mandatory = $true)]
    [string]
    $configuration
)

Write-Output "Switching file into other environment"
Write-Output "scriptName = $fileName"
Write-Output "configuration = $configuration"
```

Szukamy pliku w tym samym folderze według konwencji `NazwaPlikuBazowego.Konfiguracja.rozszerzenie`

```powershell
$fileBase = [System.IO.Path]::GetFileNameWithoutExtension($fileName) # Nazwa pliku "bazowego" jest częścią wspólną z plikiem docelowym.
$fileExtension = [System.IO.Path]::GetExtension($fileName) # Rozszerzenie się przyda później
$fileBaseBackup = [System.IO.Path]::GetTempFileName() # Zrobimy też kopię oryginalnego pliku
$workingDirectory = [System.IO.Path]::GetDirectoryName($fileName)

# Szukamy pliku według konwencji - nazywa się tak samo jak plik bazowy + .Konfiguracja i ma to samo rozszerzenie
$configurationSpecificFile = [System.IO.Path]::Combine($workingDirectory, "$fileBase.$configuration$fileExtension")

Write-Output "Configuration Specific File: $configurationSpecificFile"
Write-Output "Backup of Default file at: $fileBaseBackup"

```

Nadpisujemy blik bazowy tym specyficznym dla konfiguracji.

```powershell
try {

    if (Test-Path $configurationSpecificFile) {
		if(Test-Path $fileName)
		{
			Write-Output "Backing $fileName"
			Move-Item $fileName $fileBaseBackup -Force
		}
        Copy-Item $configurationSpecificFile $fileName
    }
    else {
        Write-Output "Configuration specific file '$configurationSpecificFile' does not exists. No substitution will be done"
    }
}
catch {
    $ex = $_.Exception.Message
    Write-Output "Ended with errors:"
    Write-Output $ex
    Write-Output "Reverting file: $fileName"
    Move-Item $fileBaseBackup $fileName -Force
}
finally {
    if (Test-Path $fileBaseBackup) {
        Remove-Item $fileBaseBackup
    }
}

```

W Post-Build podmieniamy natomiast plik konfiguracją nazwaną "Default" - plik PostScript.Default.sql powinien być taki sam jak PostScript.sql - unikniemy w ten sposób ciągle zmieniającego się pliku. Moglibyśmy również użyć tymczasowego pliku na przechowanie zawartości domyślnej - w ten sposób unikniemy też przechowywania dwóch takich samych plików w naszym repo.

Aby przetestować nasze rozwiązanie, dodajmy plik PostScript.Debug.sql, który powinien być wykonany tylko w przypadku tejże konfiguracji.

```sql
:r .\TagConfiguration.sql
:r .\TestData.sql

IF NOT EXISTS (SELECT TOP 1 Id FROM Topic WHERE Id = 100)
BEGIN
	INSERT INTO Topic (Id, Title, Url) VALUES (100, 'DEBUG','localhost');
END;
```

## Synchronizacja z Kodem

W momencie jak to piszę, Entity Framework Core (dalej EF) jeszcze nie wspiera relacji wiele do wielu zbyt dobrze ([link do github](https://github.com/aspnet/EntityFramework/issues/1368)).
Będziemy musieli zadowolić się obiektem pośredniczącym. Zobaczymy jednak jak szybko można coś wygenerować. Dalszy opis zakłada, że znasz podstawy EF oraz wiesz czym jest obiekt kontekstu `DbContext`

Listę linków z tagami wypiszemy w najprostrzej aplikacji ASP.NET Core (MVC). Po utworzeniu projektu zaczniemy od wygenerowania modelu naszej bazy. Jeżeli w ten sposób utworzymy nowy projekt, to Entity Framework Core wraz z kilkoma bibliotekami komplementarnymi jest już w zależnościach projektu. Niemniej, gdyby okazało się, że w twoim projekcie czegoś brakuje, upewnij się, że odpowiednie komponenty są zainstalowane:

```powershell
dotnet add package Microsoft.EntityFrameworkCore --version 3.1.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 3.1.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 3.1.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 3.1.0
```

Paczka `Design` przychodzi nam z pomocą przez dodatkowe możliwości CLI dotnet - dzięki niej będziemy w stanie wygenerować nasze klasy ORM zamiast ich pisać.
SqlServer definiuje implementację klienta, którą użyjemy.

Dodajmy do appsettings.json nasz Connection String:

```json
{
  "...": "...",
  "ConnectionStrings": {
    "DefaultDatabase": "Server=(localdb)\\mssqllocaldb;Database=TopicalTags;Integrated Security=True"
  }
}
```

I skorzystamy z narzędzi EF do wygenerowania naszego modelu. Ponieważ w typowej aplikacji będziemy to robić wielokrotnie, stwórzmy odrazu mały skrypt:

```powershell
# Update-Model.ps1

dotnet ef dbcontext scaffold `
   name=DefaultDatabase `
   Microsoft.EntityFrameworkCore.SqlServer `
   --output-dir Model `
   --context TopicContext `
   --force --verbose
```

Opis:

- [dotnet ef dbcontext scaffold] Używamy możliwości CLI dotnet, aby:
- [name=DefaultDatabase] - dla połączenia o nazwe DefaultDatabase (można tutaj podać też ConnectionString)
- [Microsoft.EntityFrameworkCore.SqlServer] - dostawcy połączenia dla MS SqlServer
- [--output-dir Model] - w folderze Model
- [--context TopicContext] - stworzyć implementację kontekstu TopicContext i klas modelu wygenerowanych na podstawie bazy
- [--force] - i wymusić aktualizację w przyszłości, gdy pliki będą istnieć
- [--verbose] - oraz dostać dodatkowe informacje na temat procesu.

Po uruchomieniu skryptu otrzymujemy wygenerowane klasy per tabela oraz naszą implementację kontekstu EF.
W przypadku aplikacji ASP.NET Core wystarczy jeszcze dodać nasz kontekst do kontenera IoC w Startup.cs.

```csharp
public class Startup
{
    //...

    public void ConfigureServices(IServiceCollection services)
    {
        // ...
        services.AddDbContext<TopicContext>();
    }

    // ...
}
```

Nasz kontekst zostanie wstrzyknięty automatycznie tam, gdzie o niego poprosimy. Zaimplementujmy prostą logikę zwracania danych w HomeController.Index:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TopicalTagsWebTest.Model;

namespace TopicalTagsWebTest.Controllers
{
    public class HomeController : Controller
    {
        private TopicContext Repo { get; }

        public HomeController(TopicContext repo)
        {
            Repo = repo;
        }

        public IActionResult Index()
        {
            var allTopics = Repo.Topic
                .Include(t => t.TopicTags)
                .ThenInclude(t => t.Tag)
                .ToList();
            return View(allTopics); // To simplify the Example, we just return the Entity object.
        }
    }
}
```

Wyświetlmy je na widoku Home/Index.cstml

```html
@{ ViewData["Title"] = "Home Page"; @using TopicalTagsWebTest.Model }
<div>
  <ul>
    @foreach (Topic topic in Model) {
    <li>
      <a href="@topic.Url" title="@topic.Url">@topic.Title</a>
      @foreach (Tag tag in topic.TopicTags.Select(t => t.Tag)) {
      <span class="badge badge-primary">@tag.Name</span>
      }
    </li>
    }
  </ul>
</div>
```

### Konfiguracja z życia wzięta

SqlPackage jest całkiem rozbudowanym narzędzeniem i może służyć do rozwiązania niejednego problemu. W jednym z projektów mamy środowisko Dev, QA i Prod no i swoją lokalną bazkę. Bazy siedzą na współdzielonym serwerze, gdzie nie możemy sobie pozwolić na ustawienie w profilu "Always re-create database", ponieważ nie dostaliśmy najzwyczajniej ku temu uprawnień. Naszym osobistym wymaganiem jest jednak, aby baza DEV była "czysta" przy każdym _Releasie_ i testy systemowe były wykonane na takiej właśnie wersji. QA natomiast powinien być aktualizowany bez utraty danych. Na Produkcji (na dzień w który to pisze)... nie mamy póki co zgody i uprawnień do stosowania automatycznego procesu. W tym przypadku generujemy skrypt _różnicowy_ dla bazy produkcyjnej i dostarczamy komuś z plakietką DBA (który na dodatek siedzi w biurze o parę stref czasowych później - nice).

We wszystkich przypadkach korzystamy z _Windows Authentication_ w połączeniu z bazą, ale gdyby zaszła potrzeba przekazania ukrytych haseł w automatycznym procesie, to SqlPackage również pozwala na to dla `/Action:Publish` (patrz parametr `/TargetConnectionString` lub krótko `/tcs`).

W powyższym scenariuszu najłatwiejszy się wydaje QA i własna baza - SqlPackage wykorzystujemy tak jak w powyższym Powershellu, przy czym dla własnej bazy może będziemy chcieli zastosować opcję "Always re-create database" w zależności od potrzeb. Dla QA będzie to zwyczajna aktualizacja.

Dla Produkcji będziemy stosować opcję z generowaniem skryptu:

```powershell
# GnerateDiffScript.ps1
param (
    [Parameter(Mandatory = $true)]
    [string]
    $profile,
    [Parameter(Mandatory = $true)]
    [string]
    $dacpac,
    [string]
    $output
)
$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"

if($output -eq '' -or $output -eq $null){
    $output = $profile.Replace(".xml",".sql")
}

&$sqlPackageExe /Action:Script  /SourceFile:"$dacpac" /Profile:"$profile" /OutputPath:"$output"
```

```powershell
# Example execution in your Release system (TFS, Octopus, Jenkins whatever), just adjust your paths.
.\GenerateDiffScript.ps1 -profile "TopicalTags.publish.xml" -dacpac "./TopicalTags.dacpac" -output "./GoracySkryptDlaDibieja.sql"
```

Do rozważenia jest również opcja pozwalająca na stworzenie obrazu struktury bazy danych (DACPAC z istniejącej bazy), co pozwoli nam generować skrypt odwracający nasze zmiany w razie gdyby pełny backup byłby przerostem formy nad treścią (ktoś powie, że zawsze lepiej robić backup - zgoda, ale nie zawsze jest korzystniej przywracać taką kopię w razie gdyby okazało się, że trzeba cały system przywracać do starej wersji po chwili od jego uruchomienia).

**Co zrobiliśmy z Devem na ograniczonych uprawnieniach?**
A no stworzony został drugi projekt SQLowy, który zawierał dokładnie i literalnie NIC. W ustawieniach profilu należało ustawić kilka ustawień odpowiadających za DROP obiektów, przede wszystkim `DropObjectsNotInSource` i zastosować taki profil przed naszą główną aktualizacją. Tym samym baza danych była czyszczona i stawiana na nowo bez jej usuwania i tworzenia, co by wymagało dodatkowych uprawnień.

```xml
<!-- Truncate.publish.xml -->
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <IncludeCompositeObjects>True</IncludeCompositeObjects>
    <TargetDatabaseName>Dev</TargetDatabaseName>
    <DeployScriptFileName>CleanThisIntoAbyss.sql</DeployScriptFileName>
    <TargetConnectionString>
        Data Source=DevServer;Integrated Security=True;
    </TargetConnectionString>
    <BlockOnPossibleDataLoss>False</BlockOnPossibleDataLoss>
    <DropObjectsNotInSource>True</DropObjectsNotInSource>
    <ProfileVersionNumber>1</ProfileVersionNumber>
    <BlockOnPossibleDataLoss>False</BlockOnPossibleDataLoss>
    <DoNotDropUsers>True</DoNotDropUsers>
    <CreateNewDatabase>False</CreateNewDatabase>
    <DoNotDropCredentials>True</DoNotDropCredentials>
    <DoNotDropDatabaseRoles>True</DoNotDropDatabaseRoles>
    <DoNotDropDatabaseScopedCredentials>True</DoNotDropDatabaseScopedCredentials>
    <DoNotDropLogins>True</DoNotDropLogins>
    <DoNotDropPermissions>True</DoNotDropPermissions>
    <DoNotDropRoleMembership>True</DoNotDropRoleMembership>
    <DoNotDropServerRoleMembership>True</DoNotDropServerRoleMembership>
    <DoNotDropServerRoles>True</DoNotDropServerRoles>
    <DoNotDropUsers>True</DoNotDropUsers>
  </PropertyGroup>
</Project>
```

W naszym przypadku nie usuwaliśmy rzeczy związanych z loginami i użytkownikami, bo sobie usunąłem w ten sposób użytkownika db_owner, na którym właśnie szedł proces aktualizacji :D

## Database First z Migracjami

### Aktualizacja i Automatyzacja

Jeżeli stresuje Cię, że oddajesz zadanie aktualizacji bazy danych w ręcę "tempego" narzędzia, to do powyższego rozwiązania można również dodać bardziej "jawne" migracje.
Wykorzystamy do tego celu [DbUp](https://www.nuget.org/packages/dbup-sqlserver/). DbUp wykonuje zdefiniowaną listę skryptów SQL na docelowej bazie danych, zapisując przy te już wykonane w tabeli `Journal`. Pozwala to na kontrolę migracji wykonanych już na danej bazie. Wszystko przypomina podejście _Code First_, tyle, że migracje są napisane w czystym SQL.

Stwórzmy aplikację konsolową .NET Core i zainstalujmy potrzebne narzędzia:

```powershell
dotnet add package dbup-sqlserver --version 4.3.1
dotnet add package Newtonsoft.Json --version 12.0.3
```

DbUp pozwala konfigurować sposób migracji bazy danych na wiele sposobów przez Fluent API. Napiszmy klasę DbMigrator, aby zdefiniować nasz proces.

```csharp
using DbUp;
using DbUp.Engine;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace TopicalTagsMigrations
{
    public class DbMigrator
    {
        public void Migrate(string appSettingsPath, string connectionStringName, string migrationsDirectory)
        {
            string connectionString = GetConnectionString(appSettingsPath, connectionStringName);
            DbUp.Engine.DatabaseUpgradeResult migration = MigrateWithEngine(migrationsDirectory, connectionString);
            Console.WriteLine($"Database Migration Successful: {migration.Successful}");
        }

        protected virtual DatabaseUpgradeResult MigrateWithEngine(string migrationsDirectory, string connectionString)
        {
            var engine = DeployChanges.To
                            .SqlDatabase(connectionString)
                            .WithScriptsFromFileSystem(migrationsDirectory)
                            .LogToConsole()
                            .Build();

            var migration = engine.PerformUpgrade();
            return migration;
        }
    }
}

```

Metoda GetConnectionString wyciągnie szczegóły połączenia z appsettings.json naszej aplikacji webowej, żeby nie trzymać tego w wielu miejscach:

```csharp
protected virtual string GetConnectionString(string appSettingsPath, string connectionStringName)
{
    var config = JValue.Parse(File.ReadAllText(appSettingsPath));
    string connectionString = config["ConnectionStrings"][connectionStringName].ToString();
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new ArgumentException($"The name of the connection string {connectionStringName} is not found in {appSettingsPath} > ConnectionStrings section",
            nameof(connectionStringName));
    }
    return connectionString;
}
```

Migratora uruchomimy w Program.cs

```csharp
class Program
{
    static void Main(string[] args)
    {
        ValidateArguments(args);

        DbMigrator migrator = new DbMigrator();

        migrator.Migrate(appSettingsPath: args[0], connectionStringName:args[1], migrationsDirectory:args[2]);
    }

    private static void ValidateArguments(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentOutOfRangeException(nameof(args),
                "usage: TopicalTagsMigrations.exe 'Path/To/appsettings.json' 'ConnectionStringName' 'Migration/scripts/directory'");
        }
        if (!File.Exists(args[0]))
        {
            throw new FileNotFoundException($"The app settings file in first argument does not exist: {args[0]}");
        }
        if (!Directory.Exists(args[2]))
        {
            throw new DirectoryNotFoundException($"The scripts directory in second argument does not exist: {args[1]}");
        }
    }
}
```

Możliwości wykorzystania tego są dwie:

1. Możemy zrezygnować z projektu SQL i trzymać wersję bazy danych tylko w formie migracji
2. Możemy wykorzystać projekt SQL do trzymania całościowej struktury bazy danych (widok z lotu ptaka) i wykorzystać wspomniany SqlPackage do generowania migracji!

Idąc tropem drugiej opcji będziemy chcieli trzymać migracje w folderze `Migrations` naszego projektu DbUp. Do wykonania migracji wystarczyło by wywołać więc:

```powershell
# RunMigrations.ps1
dotnet run "../TopicaltagsWebTest/appsettings.json" "DefaultDatabase" "./Migrations/"
```

Do wygenerowania migracji potrzebujemy wykonać SqlPackage z akcją _Script_ i zapisać plik według wybranej konwencji, aby zachować kolejność. Zapiszmy to w znów w Powershellu:

```powershell
#AddMigration.ps1
param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]
    $migration,
    [string]
    $migrationsFolder = "./Migrations"
)

function Get-MigrationFilePath()
{
    param([string]$migrationName, [string]$migrationsLocation)

    # Locate folder with migrations
	if(-not [System.IO.Path]::IsPathRooted($migrationsLocation)){
        $location = Get-Location
        $migrationsLocation = [System.IO.Path]::Combine($location, $migrationsLocation)
    }

    # Find highest number prefix in existing migrations
	$allMigrations = [System.IO.Directory]::GetFiles($migrationsLocation, "*.sql")

	$maxMigration = 0;
	foreach($existingMigration in $allMigrations)
	{
		$existingMigrationName = [System.IO.Path]::GetFileNameWithoutExtension($existingMigration)
		$existingMigrationNumber = 0;
		if([int]::TryParse($existingMigrationName.Substring(0,4).TrimStart('0'),  [ref] $existingMigrationNumber)){
			$maxMigration = [Math]::Max($maxMigration, $existingMigrationNumber)
		}
	}

    # Generate a name for the new migration with a higher prefix.
	$migrationNumberValue = ($maxMigration+1).ToString().PadLeft(4,'0')
	$migrationFile = [System.IO.Path]::Combine($migrationsLocation,"$migrationNumberValue-$migrationName.sql")

	return $migrationFile
}

$outputPath = Get-MigrationFilePath -migrationName $migration -migrationsLocation $migrationsFolder

# Run migration generation
$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
&$sqlPackageExe /Action:Script `
	/SourceFile:"../TopicalTags/bin/Debug/TopicalTags.dacpac" `
	/Profile:../TopicalTags/TopicalTags.publish.xml `
	/OutputPath:$outputPath

```

Kilka wskazówek na koniec:

1. Jak wspominałem wcześniej, SqlPackage generuje skrypt ze wstawkami SQLCMD (między innymi zmiennymi). Ponieważ DbUp nie wspiera tej składni, to wygenerowaną migrację trzeba oczyścić tak od `USE [$(DatabaseName)]` włącznie w górę.
2. Prawdopodobnie też w każdej migracji nie będziemy chcieli zawierać wszystkich skryptów Pre/Post Deploy. Trzeba więc dostosować konfigurację do swoich potrzeb.
3. Pierwszą migrację wygenerujmy na pustej bazie - w ten sposób stworzymy migrację z aktualną strukturą zdefiniowaną w projekcie SQL
4. Jeżeli korzystamy z DbUp, trzeba ustalić jeden sposób w zespole na tworzenie nowej bazy danych (np. lokalnej dla dewelopera). W tej chwili postawienie bazy przez opcję _Publish_ na projekcie SQL nie stworzy tabeli przechowywującej stan wykonania wszystkich migracji. DbUp w tym przypadku będzie próbował je wykonać, pomimo, że wersja bazy może odpowiadać stanowi po najnowszej migracji.
5. Wywołanie DbUp można również dodać na starcie aplikacji - podobnie jak można to zrobić w podejściu Code First.

### Współdzielenie kodu i rozwiązywanie konfliktów

Podobnie jak wcześniej - zmiany na bazie są częścią kodu. W migracjach natomiast trzeba uważać, żeby przy powtarzających się numerach porządkowych nie były wykonywane akcje będące w konflikcie. Najprawdopodobniej jednak, jeżeli pozostawimy projekt SQLowy jako część rozwiązania, to konflikt będzie widoczny gołym okiem w plikach tego projektu.

### Dywersyfikacja Środowiska

DbUp nie wymusza bardzo ścisłej konwencji tak jak na przykład [Roundhouse](https://github.com/chucknorris/roundhouse). Wykonanie każdego skryptu możemy uzależnić od czegokolwiek, np. ustawienia w App.config/appsettings.json lub zmiennej kompilacji. Jesteśmy w stanie niektóre skrypty wykonać raz, inne zawsze przy każdej migracji.

Możemy nanieść na przykład taką zmianę na migratora:

```csharp
public void Migrate(string appSettingsPath, string connectionStringName, string migrationsDirectory)
{
    string connectionString = GetConnectionString(appSettingsPath, connectionStringName);
    DbUp.Engine.DatabaseUpgradeResult executionResult = MigrateWithEngine(migrationsDirectory, connectionString);
    Console.WriteLine($"Database Migration Result: {executionResult.Successful}");

    executionResult = ExecuteAlways(migrationsDirectory, connectionString);
    Console.WriteLine($"Database Always Executed Script Result: {executionResult.Successful}");
}

protected virtual DatabaseUpgradeResult MigrateWithEngine(string migrationsDirectory, string connectionString)
{
    var engine = DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(migrationsDirectory)
                    .LogToConsole()
                    .Build();

    var migration = engine.PerformUpgrade();
    return migration;
}

protected virtual DatabaseUpgradeResult ExecuteAlways(string migrationsDirectory, string connectionString)
{
    var engine = DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsFromFileSystem(Path.Combine(migrationsDirectory, "/Always/"))
                    .JournalTo(new NullJournal())
                    .LogToConsole()
                    .Build();

    var migration = engine.PerformUpgrade();
    return migration;
}
```

Podejście będzie zdeterminowane przez wymagania konkretnego projektu i ograniczone wyobraźnią.

## Code First (Entity Framework)

Nie widziałem statystyk na ten temat, jednak odnoszę wrażenie, że metoda Code First jest najpopularniejsza we wszystkich nowych projektach. Są jednak przeciwnicy tego rozwiązania, którzy będą przeciwko abstrahowania struktury bazy danych w języku o innym przeznaczeniu. Niekoniecznie przeciwni będą tylko administratorzy Baz Danych w obawie o stanowisko ;) W tych zarzutach jest dużo racji, niemniej jest to metoda o dużych możliwościach. Jak sama nazwa wskazuje, źródłem prawdy o bazie danych jest kod aplikacji - klasy reprezentujące naszą bazę. Najpierw je piszemy, następnie na ich podstawie generujemy zmiany na bazie.

Oczywiście odzwierciedlenie bazy danych w klasach C# nie jest tak jednoznaczne, dlatego będziemy musieli trzymać się konwencji nazewnictwa lub dodatkowo konfigurować konteks, aby oddać wszystkie aspekty bazy danych.
Zacznijmy od pustego projektu _Class Library_ - TopicalTagsCodeMigrations .

```powershell
dotnet add package Microsoft.EntityFrameworkCore --version 2.2.6
dotnet add package Microsoft.EntityFrameworkCore.Design --version 2.2.6
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 2.2.6
dotnet add package Microsoft.Extensions.Configuration.Json --version 2.2.0
```

Stworzymy również nową bazę danych dla celów testowych.
Zmieniam w appsettings.json projektu Webowego:

```json
{
  "...": "...",
  "ConnectionStrings": {
    "DefaultDatabase": "Server=(localdb)\\mssqllocaldb;Database=TopicalTagsCodeFirst;Integrated Security=True"
  }
}
```

I kopiuję go do mojego nowego projektu.

### Aktualizacja

Standardowo klasy pisalibyśmy od zera, natomiast ja pójdę na skróty i skopiuje sobie klasy wygenerowane w projekcie. Kopiuję klasy Tag, Topic, TopicTags i udaję, że je właśnie napisałem ;)
Zauważmy, że w kodzie wygenerowanym wcześniej, TopicContext zawiera jeszcze dużo dodatkowej konfiguracji. Możemy ją tworzyć właśnie w metodzie DbContext.OnModelCreating lub też w wielu przypadkach poradzić sobie za pomocą atrybutów do określenia kluczy głównych, ograniczeń niektórych propercji itd.

W tym przypadku dodajmy na naszych klasach odpowiednie atrybuty Key oraz MaxLength.

```csharp
public partial class Topic
{
    public Topic()
    {
        TopicTags = new HashSet<TopicTags>();
    }

    [Key]
    public int Id { get; set; }

    [MaxLength(2000)]
    public string Title { get; set; }

    [MaxLength(2000)]
    public string Url { get; set; }

    public virtual ICollection<TopicTags> TopicTags { get; set; }
}

public partial class Tag
{
    public Tag()
    {
        TopicTags = new HashSet<TopicTags>();
    }

    [Key]
    public int Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; }

    public virtual ICollection<TopicTags> TopicTags { get; set; }
}
```

Klasę kontekstu dodamy ręcznie. Ponieważ wszystko definiujemy w _Class Library_ a nie w projekcie głównym, musimy ręcznie wskazać EF bazę danych na której pracujemy.
Możemy w tym celu zaimplementować domyślny konstruktor podający domyślny _Connection String_ lub trzymać w projekcie implementację `IDesignTimeDbContextFactory`, która z `appsettings.json` wyciągnie informacje na temat naszego połączenia.

```csharp
 public class TopicContext : DbContext
{
    public TopicContext() : base()
    {

    }

    /// <summary>
    /// This constructor will be used with Service Provider
    /// </summary>
    /// <param name="options"></param>
    public TopicContext(DbContextOptions<TopicContext> options) : base(options)
    {

    }

    public DbSet<Topic> Topics { get; set; }
    public DbSet<Tag> Tags { get; set; }

}

/// <summary>
/// This class is required for the design time tools to work on the Class Library.
/// When in a ASP.NET project, this is automatically determined by the Service Provider, so need to have this there.
/// Here thought we need to provide a source of our connection string - we will use App Settings.
/// </summary>
public class TopicContextFactory : IDesignTimeDbContextFactory<TopicContext>
{
    public TopicContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder();

        string connectionName = Environment.GetEnvironmentVariable("CONNECTION_NAME") ?? "DefaultDatabase";
        configurationBuilder.AddJsonFile("appsettings.json");
        IConfiguration configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString(connectionName);
        var optionsBuilder = new DbContextOptionsBuilder<TopicContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TopicContext(optionsBuilder.Options);
    }
}
```

Następnie polecenie `dotnet ef migrations add Initial` wygeneruje nam pierwszą migrację z kodu.
Moglibyśmy już ręcznie zaktualizować bazę danych przez `dotnet ef database update`. Możemy też skorzystać z możliwości aktualizacji bazy danych przy starcie aplikacji. To popularne rozwiązanie w _EF Code First_ ma jednak słabe strony:

- Proces który logicznie widzielibyśmy jako część ciągłej dostawy, staje się częścią działania samej aplikacji - instalując nową wersję na serwerze nie widzimy, czy aktualizacja przebiegła pomyślnie dopóki nie zaczniemy uruchamiać aplikacji;
- Wykonanie migracji będzie w wielu przypadkach robione na użytkowniku, który jednocześnie służy do normalnej obsługi aplikacji - potrzebuje on wtedy uprawnień na poziomie _db_owner_ ale w normalnym działaniu aplikacji nie chcielibyśmy funkcjonować na tak wszechstronnym koncie.

Dlatego zastosujemy podejście wywołania z konsoli.
Dla porządku zmieńmy nasz `TopicContext` dodając słowo `partial` i w nowym pliku `TopicalContext.Seed.cs` dodajmy metody dbające o dodanie danych testowych:

```csharp
public partial class TopicContext
{
    public void OnSeed()
    {
        Dictionary<int, Tag> tags = new Dictionary<int, Tag>();

        tags.Add(1, new Tag("Answers"));
        tags.Add(2, new Tag("Worldview"));
        tags.Add(3, new Tag("Christianity"));
        tags.Add(4, new Tag("Science"));
        tags.Add(5, new Tag("Biology"));
        tags.Add(6, new Tag("Plants"));
        tags.Add(7, new Tag("Astronomy"));
        tags.Add(8, new Tag("Age of the Universe"));
        tags.Add(9, new Tag("Evolution"));
        tags.Add(10, new Tag("Origin of Life"));
        tags.Add(11, new Tag("New One"));
        tags.Add(12, new Tag("And another one"));

        UpsertTags(tags.Values.ToList());

        if (!this.Topics.Any())
        {
            this.Topics.AddRange(
                    new Topic("Origin of Life Problems for Naturalists", "https://answersingenesis.org/origin-of-life/origin-of-life-problems-for-naturalists/")
                    .AddTags(tags[1], tags[9], tags[10]),

                    new Topic("Power Plants", "https://answersingenesis.org/biology/plants/power-plants/")
                    .AddTags(tags[1], tags[3], tags[5], tags[6]),

                    new Topic("Evidence for a Young World", "https://answersingenesis.org/astronomy/age-of-the-universe/evidence-for-a-young-world/")
                    .AddTags(tags[1], tags[4], tags[7], tags[8]),

                    new Topic("Are Atheists Right? Is Faith the Absence of Reason/Evidence?", "https://answersingenesis.org/christianity/are-atheists-right/")
                    .AddTags(tags[1], tags[2], tags[3])
                    );

        }

        this.SaveChanges();
    }

    private void UpsertTags(List<Tag> tags)
    {
        var keys = tags.Select(t => t.Name).ToList();
        var existingTagNameId = this.Tags
            .Where(t => keys.Contains(t.Name))
            .ToDictionary(t => t.Name, t => t.Id);

        tags.ForEach(t =>
        {
            // Get existing Ids
            if (existingTagNameId.ContainsKey(t.Name))
            {
                t.Id = existingTagNameId[t.Name];
            }
        });

        this.AddRange(tags
            .Where(t => !existingTagNameId.ContainsKey(t.Name)));
    }
}
```

### Automatyzacja

Zespół EF zdaje się nie do końca jeszcze mieć przemyślanej kwestii aktualizacji bazy danych z konsoli ([https://github.com/aspnet/EntityFramework.Docs/issues/807]). W sieci można znaleźć kilka rozwiązań tegoż problemu z wykorzystaniem dziwnych wywołań z plikiem ef.dll

```powershell
dotnet exec --depsfile MyApp.deps.json --runtimeconfig MyApp.runtimeconfig.json ef.dll database update
```

To rozwiązanie nie przypadło mi do gustu, dlatego proponuję wywołanie migracji przez aplikację webową, ale z flagą przekazaną na starcie. W końcu ASP.NET Core jest w gruncie rzeczy zwykłą konsolówką.
Potraktujmy ją tak. Zamkniemy funkcjonalność migracji w zgrabną klasę.
Najpierw inicjalizujemy migratora konfiguracją z pliku:

```csharp
public class DbMigrator
{
    public static DbMigrator WithDefaultConfiguration()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Console.WriteLine($"Using default appsettings.json configuration file along with environmental configs. Environment: {environmentName ?? "Default"}");

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();

        return new DbMigrator(config);
    }

    public DbMigrator(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    private IConfiguration Configuration { get; set; }
}
```

Potem rozszerzmy o inicjalizację kontekstu:

```csharp
public class DbMigrator : IDisposable
{
    public static DbMigrator WithDefaultConfiguration() { /*...*/ }

    public DbMigrator(IConfiguration configuration){ /*...*/ }

    private IConfiguration Configuration { get; set; }

    private const string DefaultConnectionStringName = "DefaultDatabase";

    private TopicContext Context { get; set; }

    public DbMigrator UsingConnectionStringName(string connectionStringName)
    {
        // Typical EF Core initialization.
        DbContextOptionsBuilder<TopicContext> options = new DbContextOptionsBuilder<TopicContext>();
        options.UseSqlServer(this.Configuration.GetConnectionString(connectionStringName));
        Context = new TopicContext(options.Options);

        return this;
    }

    private void EnsureContextInitialized()
    {
        if (Context == null)
        {
            Console.WriteLine("Initializing Context");
            UsingConnectionStringName(DefaultConnectionStringName);
        }
    }

    public void Dispose()
    {
        if (Context != null)
        {
            Context.Dispose();
        }
    }
}
```

I na koniec dodajmy metody do migracji i "zasiewania" danych:

```csharp
public class DbMigrator : IDisposable
{
    public static DbMigrator WithDefaultConfiguration() { /*...*/ }

    private const string DefaultConnectionStringName = "DefaultDatabase";

    private TopicContext Context { get; set; }

    private IConfiguration Configuration { get; set; }

    public DbMigrator(IConfiguration configuration){ /*...*/ }

    public void MigrateDatabase()
    {
        Console.WriteLine("Migrating database started");
        EnsureContextInitialized();
        this.Context.Database.Migrate();
    }

    public void RunDataSeed()
    {
        Console.WriteLine("Data Seed");
        EnsureContextInitialized();
        Context.OnSeed();
    }

    private void EnsureContextInitialized(){ /*...*/ }

    public DbMigrator UsingConnectionStringName(string connectionStringName){ /*...*/ }

    public void Dispose(){ /*...*/ }
}
```

Jeżeli sytuacja tego wymaga, możemy dodać specjalny _Connection String_ używany do migracji i podać go do migratora jako parametr. Całość wywołujemy w Program.cs naszego projektu webowego:

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        if (args.Any(a => a.Equals("--migration")))
        {
            using (DbMigrator migrator = DbMigrator.WithDefaultConfiguration()
                .UsingConnectionStringName("MigrationConnectionString"))
            {
                migrator.MigrateDatabase();
                migrator.RunDataSeed();
            }
        }
        else
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }

    //...
}
```

Jeżeli dalej chcemy wykonywać aktualizację na starcie aplikacji, wystarczy użyć tego samego `DbMigrator` w Startup.cs.

1. Startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
      .AddDbContext<TopicalTagsCodeMigrations.Model.TopicContext>
        (options => options .UseSqlServer(this.Configuration
           .GetConnectionString("DefaultDatabase")));
}
```

2. HomeController.cs -> zamiast `using TopicalTagsWebTest.Model;` będzie `using TopicalTagsCodeMigrations.Model;`

3. Index.cshtml -> podobnie zamiast `@using TopicalTagsWebTest.Model` będzie `@using TopicalTagsCodeMigrations.Model`

Następnie możemy dodać wywołanie migracji w Startup.cs

```csharp
public void Configure(IApplicationBuilder app,
    IHostingEnvironment env)
{
    //...
    InitializeDatabase(app);
}

private void InitializeDatabase(IApplicationBuilder app)
{
    // AddDbContext is creating a Scoped lifetime context
    // In this case we do not have a Request scope, so let's create
    // our own.
    using (var scope = app.ApplicationServices.CreateScope())
    {
        using (var ctx = scope.ServiceProvider
            .GetService<TopicalTagsCodeMigrations.Model.TopicContext>())
        {
            ctx.Database.Migrate();
        }
    }
}
```

### Rozwiązywanie Konfliktów

W porównaniu z starszym EF, tym razem obraz naszego modelu nie jest trzymany w jakimś przydługawym i skompresowanym zasobie, ale w formie klasy C#.
W razie stwierdzenia poważnego konfliktu, kiedy dwóch programistów zmienia dokładnie to samo, postępowanie jest następujące:

1. Cofnij Merge'a
2. Usuń swoją migrację przez `dotnet ef migrations remove` pozostawiając zmiany na swoim modelu.
3. Zmerguj kod kolegi
4. Utwórz swoją migrację (o ile faktycznie decydujesz się zmienić coś po koledze ;))

### Dywersyfikacja Środowiska

Podobnie jak w przypadku DbUp, tutaj mamy całkowitą swobodę synchronizacji danych przez implementację w kodzie. W tym przypadku możemy korzystać z konfiguracji w `appsettings.json` lub na przykład wstrzykniętego `IHostingEnvironment`:

```csharp
// Startup.cs
private void DataSeed(IApplicationBuilder app,
    IHostingEnvironment env)
{
    using (var scope = app.ApplicationServices.CreateScope())
    {
        using (var ctx = scope.ServiceProvider.GetService<TopicalTagsCodeMigrations.Model.TopicContext>())
        {
            ctx.OnSeed(env.IsDevelopment());
        }
    }
}
```

Znów determinuje nas tylko wymaganie projektowe i ogranicza wyobraźnia.

## Podsumowanie

Pokazaliśmy na żywych i praktycznych przykładach trzy różne podejścia do aktualizacji bazy danych, które nadają się do zastosowania w prawdziwych projektach (=w każdym przypadku podejmujemy i rozwiązujemy 6 wyzwań takiego bohaterskiego czynu). Nie miałem a celu faworyzować któregokolwiek podejścia, ale dać praktyczną wiedzę na temat rozwiązań, które można zastosować.

Używałem każdego z tych rozwiązań, ale ważne jest, aby wybrać jedną konwencję, trzymać się jej i dostosować do swoich potrzeb.

### Database First

Wykorzystując potężne narzędzie SqlPackage jesteśmy w stanie w elegancki sposób implementować naszą bazę, automatyzować aktualizację prostym wywołaniem konsolowym, rozwiązywać konflikty z tą samą prostotą jak dla reszty kodu i dywersyfikować skrypty względem środowiska. Z pomocą Entity Framework bez problemu również przenosimy nasze tabulki do kodu biznesowego w postaci klas.

### Database First z Migracjami

Dodając do rozwiązania wykonanie migracji z DbUp byliśmy w stanie również wzbogacić rozwiązanie o większą kontrolę nad zmianami, które wykonujemy na bazie iteracyjnie zachowując przy tym wszystko co mieliśmy w poprzednim rozwiązaniu.

### Code First

Poświęciliśmy temu rozwiązaniu stosunkowo mało czasu ze względu na popularność różnych blogów i kursów na ten temat, jednak dla tych, którzy chcą mieć z SQL jak najmniej do czynienia jest to również rozwiązanie szeroko stosowane.
Aktualizujemy i automatyzujemy korzystając z wywołań CLI lub bezpośrednio w kodzie. Fakt ten również daje nam bezproblemowo dywersyfikować stan naszej bazy w zależności od w zasadzie jakiegokolwiek warunku. Nowe EF ulepsza również podejście do rozwiązywania konfliktów w współdzielonych repozytoriach.

## Usprawiedliwenie

Pora rozgrzeszyć wspomniany we wstępie projekt. Chociaż moje wejście do zespołu nie należało do najłatwiejszych, bo byłem najmłodszy. Co tu taki podrostek będzie przewracał ustanowiony porządek? Niemniej wydaje mi się, że pierwszym problemem było nadanie całemu projektowi charakteru prototypu, który przegapił moment stania się faktycznym przedsięwzięciem a drugim swego rodzaju bezkrólewie, bo również lider był już jedną nogą i czterema palcami w innym projekcie. W pewnym momencie ktoś z góry uznał, że wchodzimy na żywo i nikt się tego nie spodziewał. Na szczęście z teamem ostatecznie doszliśmy do dobrych relacji i razem udało się rozwiązać kwestię bazy. W tym przypadku zastosowaliśmy dedykowane rozwiązanie Database First przez Tranzycje.
