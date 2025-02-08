**Instrukcja wygenerowania pliku pfx z użyciem OpenSSL**

Należy otworzyć terminal OpenSSL i wpisać poniższą komendę. Podczas interaktywnego procesu większość pól zatwierdza się Enterem, natomiast w polu Common Name należy wpisać **userml**: 

openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout client.key -out client.crt




Następnie należy przekonwertować wygenerowane pliki do formatu PFX oraz ustawiając hasło:

openssl pkcs12 -export -out client.pfx -inkey client.key -in client.crt

Po wygenerowaniu pliku client.pfx należy kliknąć go, aby uruchomić kreator importu certyfikatów, a następnie dodać certyfikat do magazynu „Osobisty” w systemie.

----------------

**Jakie problemy napotkałem podczas realizacji zadania?**
  
Podczas realizacji zadania napotkałem kilka drobnych problemów. Lokalnie aplikacja korzysta z serwera Kestrel, który sprawdza certyfikat klienta podczas połączenia HTTPS – tutaj walidacja działa bezpośrednio w kodzie. Natomiast na produkcji (w Azure) TLS termination odbywa się przez usługę ARR, która przekazuje certyfikat w nagłówku X-ARR-ClientCert. W związku z tym musiałem dodać dodatkowy middleware, który odczytuje ten nagłówek i sprawdza, czy certyfikat ma odpowiedni CN ("userml"). Aby middleware działał na produkcji, w ustawieniach Azure App Service ustawiłem zmienną środowiskową ASPNETCORE_ENVIRONMENT na Production. Dzięki temu tylko połączenia z właściwym certyfikatem są akceptowane.

Korzystając z .NET 9, zauważyłem ostrzeżenie SYSLIB0057, które informuje, że metoda ładowania certyfikatu przy użyciu konstruktora X509Certificate2(byte[]) jest przestarzała i sugeruje użycie X509CertificateLoader. Ponieważ oficjalne i stabilne API X509CertificateLoader nie jest jeszcze dostępne, postanowiłem pozostać przy użyciu konstruktor X509Certificate2(byte[])

