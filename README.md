# Wulkanizacja.Auth

## Opis
Wulkanizacja.Auth to projekt autoryzacji i uwierzytelniania użytkowników, zbudowany w oparciu o ASP.NET Core i Entity Framework Core. Projekt obsługuje rejestrację, logowanie oraz generowanie tokenów JWT.

## Funkcje
- Rejestracja użytkowników
- Logowanie użytkowników
- Generowanie tokenów JWT
- Hashowanie haseł za pomocą `PasswordHasher`
- Ograniczanie liczby żądań logowania

## Wymagania
- .NET 8
- PostgreSQL

## Konfiguracja
Plik 'appsettings.json' zawiera konfigurację połączenia z bazą danych PostgreSQL oraz konfigurację JWT
```json
 "postgres": {
   "ConnectionString": "Database=Users;Username=postgres;Enlist=False;Password=admin;Port=5432;Host=localhost;TimeZone=Europe/Warsaw"
 },

  "Jwt": {
    "Key": "FzlKp3P76ctGL7qxX98QkDmJ5bQJS/0NEBIZj8OUVaw=",
    "Issuer": "WulkanizacjaAuth",
    "ExpiresInHours": 1
  },
```
  W przypadku dockera konfiguracja polaczenia z bazą jest w pliku docker-compose.yml

## Uruchomienie
1. Sklonuj repozytorium
2. Otwórz projekt w Visual Studio
3. Skonfiguruj bazę danych PostgreSQL zgodnie z ustawieniami w appsettings.json.
4. Uruchom aplikację za pomocą Visual Studio 2022.
5. Migracje zostaną zastosowane automatycznie podczas uruchamiania aplikacji.
6. Teraz można testować endpointy z załączonej kolekcji postmana lub przez API swaggera pod adresem http://localhost:5020/swagger. Dla użytkowników preferujących interfejs graficzny, dostępna jest także aplikacja desktopowa, która umożliwia wygodne korzystanie z funkcjonalności API.

## Uruchomienie Alternatywne Docker
1. Zainstaluj program Docker Desktop
2. Wejdz do folderu Wulkanizacja.Auth\Wulkanizacja.Auth >> tutaj gdzie plik csproj
3. Skonfiguruj połączenie z bazą PostgreSQL w pliku docker-compose.yml
3. Uruchom cmd z poziomu folderu
4. Wpisz docker-compose build a następnie docker-compose up -d
5. Teraz można testować endpointy z załączonej kolekcji postmana lub przez API swaggera pod adresem http://localhost:5020/swagger. Dla użytkowników preferujących interfejs graficzny, dostępna jest także aplikacja desktopowa, która umożliwia wygodne korzystanie z funkcjonalności API.

## Endpointy
- `POST   /Auth/login`      - Sprawdza dane podane do logowania i zwraca token do uwierzytelnienia się z serwisem Wulkanizacja Service oraz zalogowania do aplikacji dekstopowej
- `POST   /Auth/register`   - Rejestruje nowego użytkownika w bazie

## Dokumentacja API
Dokumentacja API jest dostępna pod adresem http://localhost:5020/swagger po uruchomieniu aplikacji. Aby przetestować endpointy, można użyć załączonej kolekcji Postmana znajdującej się w katalogu Postman (Tutaj gdzie ten README)

## Autorzy
- [Aleksander Żak]
