# IoTManagement - Plataforma de Gerenciamento de Dispositivos IoT

Collaborative platform for registering IoT devices, to provide insights for planting decision-making

**Versão:** 1.0.0 (Maio 2025)
**Autor:** Fábio de Faria

## Sumário

1.  [Visão Geral do Projeto](#visão-geral-do-projeto)
2.  [Objetivos](#objetivos)
3.  [Arquitetura da Solução](#arquitetura-da-solução)
    *   [Diagrama da Arquitetura (Conceitual)](#diagrama-da-arquitetura-conceitual)
    *   [Descrição dos Projetos](#descrição-dos-projetos)
4.  [Decisões de Design e Implementação Chave](#decisões-de-design-e-implementação-chave)
    *   [Clean Architecture](#clean-architecture)
    *   [Autenticação e Autorização (OAuth 2.0)](#autenticação-e-autorização-oauth-20)
    *   [Comunicação com a API Externa (CIoTD)](#comunicação-com-a-api-externa-ciotd)
    *   [Comunicação com Dispositivos IoT (Telnet)](#comunicação-com-dispositivos-iot-telnet)
    *   [Interface do Usuário (Blazor Server)](#interface-do-usuário-blazor-server)
    *   [Simulação da API Externa (MockAPI)](#simulação-da-api-externa-mockapi)
    *   [Tratamento de Exceções](#tratamento-de-exceções)
    *   [Injeção de Dependência](#injeção-de-dependência)
    *   [Mapeamento de Dados (DTOs)](#mapeamento-de-dados-dtos)
5.  [Pilha Tecnológica](#pilha-tecnológica)
6.  [Configuração e Execução do Projeto](#configuração-e-execução-do-projeto)
    *   [Pré-requisitos](#pré-requisitos)
    *   [Executando o MockAPI (Simulador CIoTD)](#executando-o-mockapi-simulador-ciotd)
    *   [Executando a API Principal (IoTManagement.API)](#executando-a-api-principal-iotmanagementapi)
    *   [Executando a Interface do Usuário (IoTManagement.UI.Blazor)](#executando-a-interface-do-usuário-iotmanagementuiblazor)
    *   [Ordem de Inicialização](#ordem-de-inicialização)
7.  [Testes](#testes)
8.  [Credenciais de Usuário (Para Demonstração)](#credenciais-de-usuário-para-demonstração)
9.  [Melhorias e Avanços Futuros para esta Solução](#melhorias-e-avanços-futuros-para-esta-solução)
10. [Sugestões de Melhorias para a API Externa CIoTD](#sugestões-de-melhorias-para-a-api-externa-ciotd)
11. [Contribuição](#contribuição)

---

## 1. Visão Geral do Projeto

O projeto **IoTManagement** é uma solução full-stack desenvolvida em .NET 8 e C# com o objetivo de fornecer uma interface para interagir com dispositivos IoT cadastrados em uma plataforma colaborativa hipotética chamada **Community IoT Device (CIoTD)**. A solução permite que usuários autenticados visualizem dispositivos, consultem seus detalhes, explorem comandos disponíveis e executem esses comandos, recebendo os resultados formatados.

A interação com a plataforma CIoTD é simulada através de um MockAPI, e a comunicação com os dispositivos IoT para execução de comandos é realizada via protocolo Telnet.

## 2. Objetivos

*   **Interface de Usuário Intuitiva:** Fornecer uma interface web moderna e fácil de usar (Blazor) para que os usuários possam gerenciar e interagir com dispositivos IoT.
*   **Autenticação Segura:** Implementar um sistema de autenticação robusto baseado em OAuth 2.0 para proteger o acesso à API e aos recursos.
*   **Integração com Plataforma Externa:** Consumir dados de dispositivos da plataforma CIoTD (simulada).
*   **Execução de Comandos:** Permitir que usuários executem comandos em dispositivos IoT via Telnet.
*   **Visualização de Dados:** Apresentar detalhes dos dispositivos e resultados de comandos de forma clara.
*   **Código Escalável e Manutenível:** Adotar boas práticas de design e arquitetura (Clean Architecture) para facilitar a evolução e manutenção do sistema.

## 3. Arquitetura da Solução

A solução adota os princípios da **Clean Architecture** (também conhecida como Arquitetura Cebola ou Hexagonal) para promover a separação de responsabilidades, testabilidade e independência de frameworks e tecnologias de infraestrutura.

### Diagrama da Arquitetura (Conceitual)

```text
+-----------------------------------------------------------------------------------+
|                                IoTManagement.UI.Blazor (Frontend)                 |
|                                     (Apresentação)                                |
+-----------------------------------------------------------------------------------+
       ^                                      | (Consome API)
       | (HTTP/S Requests)                    v
+-----------------------------------------------------------------------------------+
|                                IoTManagement.API (Backend API REST)               |
|                                     (Interface/Adaptador)                         |
+-----------------------------------------------------------------------------------+
       ^                                      | (Usa Serviços de Aplicação)
       | (Chamadas de Serviço)                v
+-----------------------------------------------------------------------------------+
|                                IoTManagement.Application                          |
|                                     (Lógica de Aplicação, Casos de Uso, DTOs)     |
+-----------------------------------------------------------------------------------+
       ^                                      | (Depende de Interfaces de Domínio)
       | (Interfaces de Repositório/Serviço)  v
+-----------------------------------------------------------------------------------+
|                                IoTManagement.Domain                               |
|                                     (Entidades, Lógica de Domínio, Interfaces)    |
+-----------------------------------------------------------------------------------+
       ^                                      | (Implementações de Infraestrutura)
       | (Abstrações)                         v
+-----------------------------------------------------------------------------------+
|                                IoTManagement.Infrastructure                       |
|                                     (Acesso a Dados, Serviços Externos, Telnet)   |
+-----------------------------------------------------------------------------------+
       |                                      ^
       | (Comunicação com CIoTD API)          | (Comunicação Telnet com Dispositivos)
       v                                      |
+------------------------+         +--------------------------+
| IoTManagement.MockAPI  |         | Dispositivos IoT (Reais) |
| (Simulador CIoTD)      |         | (Simulados via Telnet)   |
+------------------------+         +--------------------------+
```

### Descrição dos Projetos

*   **`IoTManagement.Domain`**: Camada central da arquitetura. Contém as entidades de negócio (ex: `Device`, `User`, `DeviceCommand`), a lógica de domínio pura, exceções de domínio e as interfaces para repositórios e serviços de domínio que serão implementadas pela camada de infraestrutura. Não depende de nenhuma outra camada.
*   **`IoTManagement.Application`**: Contém a lógica de aplicação (casos de uso). Orquestra as entidades de domínio e utiliza as interfaces definidas no domínio para interagir com a infraestrutura. Define DTOs (Data Transfer Objects) para comunicação entre camadas e com a API. Depende apenas da camada `Domain`.
*   **`IoTManagement.Infrastructure`**: Implementa as interfaces definidas no `Domain` e `Application`. Lida com preocupações externas como acesso a banco de dados (se houver), comunicação com APIs de terceiros (CIoTD), implementação do cliente Telnet e outros serviços de infraestrutura (ex: `TokenService`, `CIoTDApiService`). Depende do `Domain` e `Application`.
*   **`IoTManagement.API`**: Expõe a funcionalidade da aplicação como uma API REST. Recebe requisições HTTP, as direciona para os serviços da camada `Application` e retorna as respostas. Responsável pela autenticação (OAuth 2.0 endpoints), validação de entrada e serialização/deserialização. Depende da camada `Application`.
*   **`IoTManagement.UI.Blazor`**: Interface do usuário desenvolvida em Blazor Server. Consome a `IoTManagement.API` para exibir dados e permitir interações do usuário. Contém páginas, componentes, modelos de visualização e serviços para interagir com o backend.
*   **`IoTManagement.MockAPI`**: Um projeto ASP.NET Core Minimal API que simula a API externa "Community IoT Device (CIoTD)". Utilizado para desenvolvimento e testes, permitindo que a solução principal seja desenvolvida independentemente da disponibilidade da API CIoTD real.
*   **`tests`**: Destinado a conter testes unitários, estrutura xUnit, testes unitários baseados nas melhores práticas, como *Arrange-Act-Assert* e *Mocking* com Moq.

## 4. Decisões de Design e Implementação Chave

### Clean Architecture
*   **Decisão:** Adotar a Clean Architecture.
*   **Racional:** Promove alta coesão e baixo acoplamento entre as camadas, facilitando a testabilidade (especialmente das camadas `Domain` e `Application` isoladamente), a manutenibilidade e a capacidade de trocar implementações de infraestrutura (ex: banco de dados, provedor de API externa) com mínimo impacto no núcleo da aplicação.

### Autenticação e Autorização (OAuth 2.0)
*   **Decisão:** Utilizar OAuth 2.0 (fluxo Resource Owner Password Credentials - ROPC, simplificado para este desafio) para autenticação na `IoTManagement.API`.
*   **Racional:** Padrão de indústria para autenticação segura de APIs. Permite a emissão de tokens de acesso (JWT) que são usados para autorizar requisições subsequentes. Embora ROPC seja menos ideal que outros fluxos para cenários de usuário final, ele simplifica a implementação para o escopo do desafio, focando no backend.
*   **Implementação:** O `AuthController` na `IoTManagement.API` lida com a emissão de tokens. A `IoTManagement.Infrastructure` contém o `TokenService` para geração e validação de JWTs e um `UserStore` (inicialmente em memória) para validar credenciais.

### Comunicação com a API Externa (CIoTD)
*   **Decisão:** Abstrair a comunicação com a API CIoTD através de uma interface `ICIoTDApiService` (definida na `IoTManagement.API` ou `Application`) e implementada como `CIoTDApiService` na `IoTManagement.Infrastructure`.
*   **Racional:** Desacopla a aplicação da implementação específica do cliente HTTP e da URL da API externa. Facilita a substituição por uma implementação real ou a alteração do cliente HTTP (ex: de `HttpClient` para `Refit` ou `RestSharp`) sem impactar a lógica de aplicação. Permite mockar facilmente para testes.

### Comunicação com Dispositivos IoT (Telnet)
*   **Decisão:** Implementar um `TelnetClient` (interface `ITelnetClient` no `Domain`, implementação na `Infrastructure`) para enviar comandos aos dispositivos. Um `DeviceCommandExecutionService` (interface `IDeviceCommandExecutionService` no `Domain`, implementação na `Infrastructure`) orquestra essa comunicação.
*   **Racional:** Atende ao requisito funcional de comunicação via Telnet. A abstração via interface permite testar a lógica de execução de comandos sem uma conexão Telnet real e facilita a evolução do cliente Telnet (ex: adicionar logging, re-tentativas).
*   **Formato do Comando:** Conforme especificado: `comando param1 param2 ...\r`. Resposta esperada: `string\r`.

### Interface do Usuário (Blazor Server)
*   **Decisão:** Utilizar Blazor Server para a interface do usuário.
*   **Racional:** Permite o desenvolvimento full-stack em C#, aproveitando o ecossistema .NET. O Blazor Server mantém o estado no servidor e lida com interações UI via SignalR, o que pode ser adequado para aplicações com lógica de UI mais complexa ou que se beneficiem do acesso direto a recursos do servidor (embora aqui a UI consuma a API).

### Simulação da API Externa (MockAPI)
*   **Decisão:** Criar um projeto `IoTManagement.MockAPI` para simular a CIoTD.
*   **Racional:** Permite o desenvolvimento e teste da `IoTManagement.API` e `IoTManagement.UI.Blazor` sem depender de uma API CIoTD real (que é hipotética). Garante um ambiente controlado e previsível para os dados dos dispositivos.

### Tratamento de Exceções
*   **Decisão:** Utilizar um middleware global de tratamento de exceções na `IoTManagement.API`.
*   **Racional:** Centraliza o tratamento de erros, garantindo respostas de erro consistentes e formatadas (ex: JSON com código de status HTTP apropriado) para os clientes da API, além de permitir logging centralizado de exceções não tratadas.

### Injeção de Dependência
*   **Decisão:** Utilizar o mecanismo de injeção de dependência nativo do ASP.NET Core.
*   **Racional:** Promove o baixo acoplamento e facilita a testabilidade, permitindo que as dependências sejam facilmente substituídas por mocks em cenários de teste. Configurado em `Program.cs` de cada projeto executável e em classes de extensão (ex: `DependencyInjection.cs` na `Infrastructure`).

### Mapeamento de Dados (DTOs)
*   **Decisão:** Utilizar Data Transfer Objects (DTOs) na camada de Aplicação para transferir dados entre a API e os serviços de aplicação, e entre os serviços de aplicação e a UI (via modelos da API).
*   **Racional:** Desacopla as camadas, evitando que as entidades de domínio sejam expostas diretamente para fora da aplicação ou para a UI. Permite modelar os dados especificamente para cada caso de uso ou contrato de API.

## 5. Pilha Tecnológica

*   **Linguagem:** C# 12
*   **Framework Backend:** .NET 8, ASP.NET Core 8 (para API e MockAPI)
*   **Framework Frontend:** Blazor Server (.NET 8)
*   **Comunicação API:** REST, JSON
*   **Autenticação:** OAuth 2.0 (JWT)
*   **Comunicação com Dispositivos:** TCP/IP (Telnet)
*   **Testes:** (Estrutura para xUnit, testes unitários baseados nas melhores práticas, como *Arrange-Act-Assert* e *Mocking* com Moq.)
*   **IDE Sugerida:** Visual Studio 2022, JetBrains Rider, VS Code

## 6. Configuração e Execução do Projeto

### Pré-requisitos

*   .NET 8 SDK instalado.
*   Um cliente Telnet (opcional, para testar a comunicação com dispositivos simulados fora da aplicação).
*   IDE de sua preferência.

### Executando o MockAPI (Simulador CIoTD)

1.  Abra um terminal na pasta raiz do projeto `D:\src\IoTManagement.MockAPI\`.
2.  Execute o comando: `dotnet run`
3.  Por padrão, o MockAPI estará disponível em `http://localhost:5280` e `https://localhost:7280`.
    A interface Swagger estará disponível na raiz (ex: `http://localhost:5280/`).
4.  Este serviço deve estar em execução para que a `IoTManagement.API` possa consumir os dados dos dispositivos.

### Executando a API Principal (IoTManagement.API)

1.  Certifique-se de que o `IoTManagement.MockAPI` está em execução.
2.  Abra um terminal na pasta raiz do projeto `D:\src\IoTManagement.API\`.
3.  Execute o comando: `dotnet run`
4.  Por padrão, a API principal estará disponível em `http://localhost:5000` e `https://localhost:5001` (verifique `launchSettings.json` para as portas exatas).
    A interface Swagger estará disponível em `/swagger` (ex: `http://localhost:5000/swagger`).
5.  Este serviço deve estar em execução para que a `IoTManagement.UI.Blazor` possa funcionar.

### Executando a Interface do Usuário (IoTManagement.UI.Blazor)

1.  Certifique-se de que a `IoTManagement.API` (e consequentemente o `IoTManagement.MockAPI`) está em execução.
2.  Abra um terminal na pasta raiz do projeto `D:\src\IoTManagement.UI.Blazor\`.
3.  Execute o comando: `dotnet run`
4.  Por padrão, a interface Blazor estará disponível em `http://localhost:5100` e `https://localhost:7100` (verifique `launchSettings.json` para as portas exatas).
5.  Acesse a URL no seu navegador.

### Ordem de Inicialização

1.  `IoTManagement.MockAPI`
2.  `IoTManagement.API`
3.  `IoTManagement.UI.Blazor`

## 7. Testes

O projeto `tests` (localizado em `D:\tests\`) foi previsto na estrutura para conter os testes automatizados da solução. Recomenda-se a implementação de:

*   **Testes Unitários:** Para validar a lógica isolada em classes e métodos, especialmente nas camadas `Domain` e `Application`.
*   **Testes de Integração:** Para validar a interação entre componentes, como a API com os serviços de aplicação, ou os serviços de aplicação com implementações de infraestrutura (mockando dependências externas como Telnet e a API CIoTD).

*Nota: A implementação detalhada dos testes não fez parte do escopo inicial desta entrega, mas sua estrutura está presente para evolução.*

## 8. Credenciais de Usuário (Para Demonstração)

Conforme o requisito funcional `i.i`, os usuários são considerados pré-cadastrados. Para fins de demonstração e teste, as seguintes credenciais podem ser utilizadas na interface de login:

*   **Usuário 1:**
    *   **Username:** `user1@example.com`
    *   **Password:** `Password123!`
*   **Usuário 2:**
    *   **Username:** `user2@example.com`
    *   **Password:** `Password456!`

Estas credenciais estão configuradas de forma "hardcoded" no `UserStore.cs` na camada `IoTManagement.Infrastructure` para simplificar. Em um ambiente de produção, elas seriam armazenadas de forma segura em um banco de dados.

## 9. Melhorias e Avanços Futuros para esta Solução

*   **Persistência de Dados Real:**
    *   Implementar um banco de dados (ex: PostgreSQL, SQL Server, MongoDB) para persistir usuários, tokens de refresh, e potencialmente um cache local de dispositivos ou configurações de usuário.
    *   Utilizar Entity Framework Core ou outro ORM/ODM para a camada de acesso a dados.
*   **Gerenciamento de Usuários Completo:**
    *   Implementar registro de novos usuários, recuperação de senha, gerenciamento de perfis e papéis/permissões.
*   **Cliente Telnet Robusto:**
    *   Melhorar o `TelnetClient` com tratamento de timeouts mais sofisticado, logging detalhado das interações, e possivelmente connection pooling se o volume de comandos for alto.
*   **Logging e Monitoramento Avançados:**
    *   Integrar uma solução de logging estruturado (ex: Serilog) e enviar logs para plataformas como Seq, ELK Stack ou Azure Application Insights.
    *   Adicionar monitoramento de performance e saúde da aplicação.
*   **Testes Abrangentes:**
    *   Desenvolver uma suíte completa de testes unitários e de integração.
    *   Considerar testes End-to-End (E2E) para a interface Blazor.
*   **Pipeline de CI/CD:**
    *   Configurar um pipeline de Integração Contínua e Entrega Contínua (ex: GitHub Actions, Azure DevOps) para automatizar build, testes e deployment.
*   **Internacionalização (i18n) e Localização (l10n):**
    *   Adaptar a UI e as mensagens da API para suportar múltiplos idiomas.
*   **Documentação da API Interativa Aprimorada:**
    *   Enriquecer a documentação Swagger com exemplos de requisição/resposta e descrições mais detalhadas.
*   **Notificações em Tempo Real (Opcional):**
    *   Para operações de comando mais longas, considerar o uso de SignalR (além do uso pelo Blazor Server) para notificar o cliente sobre o progresso ou conclusão, em vez de apenas esperar pela resposta HTTP.
*   **Segurança Aprimorada:**
    *   Revisar e aplicar todas as melhores práticas de segurança OWASP.
    *   Implementar Refresh Tokens para OAuth 2.0 para melhorar a gestão do ciclo de vida dos tokens de acesso.

## 10. Sugestões de Melhorias para a API Externa CIoTD

Durante o desenvolvimento e análise da API CIoTD (conforme `API.txt`), identificamos algumas oportunidades de aperfeiçoamento que poderiam beneficiar a plataforma e seus consumidores:

1.  **Aprimoramento da Segurança com OAuth 2.0 (em vez de Basic Auth):**
    *   **Proposta:** Migrar de `Basic Auth` para **OAuth 2.0**.
    *   **Benefícios:** Maior segurança, controle de acesso granular (escopos), integração facilitada, revogação de tokens.

2.  **Otimização da Listagem de Dispositivos (`GET /device`):**
    *   **Proposta:**
        *   Retornar dados essenciais dos dispositivos (não apenas IDs).
        *   Implementar paginação.
        *   Adicionar suporte a filtragem e ordenação.
    *   **Benefícios:** Melhor performance (menos requisições), escalabilidade, usabilidade aprimorada.

3.  **Padronização e Clareza nas Respostas de Erro:**
    *   **Proposta:** Adotar um formato JSON padrão para respostas de erro (`errorCode`, `message`, `details`).
    *   **Benefícios:** Melhor experiência do desenvolvedor (DX), diagnóstico facilitado.

4.  **Versionamento Explícito da API:**
    *   **Proposta:** Implementar versionamento via URI (ex: `/api/v1/device`) ou cabeçalho HTTP.
    *   **Benefícios:** Evolução controlada, manutenção simplificada, transição suave para novas versões.

5.  **Melhoria na Definição de Comandos e Formatos de Resultado:**
    *   **Proposta:**
        *   Clarificar o tipo de `Device.commands[n].command.command` (se string textual ou binário codificado).
        *   Para `Device.commands[n].format`, considerar estruturas JSON mais diretas ou tipos predefinidos para formatos comuns, além do schema OpenAPI completo.
    *   **Benefícios:** Maior interoperabilidade, validação facilitada, melhor manutenibilidade do catálogo de comandos.

6.  **Considerações sobre Taxas de Requisição (Rate Limiting):**
    *   **Proposta:** Implementar políticas de *rate limiting* com cabeçalhos HTTP informativos.
    *   **Benefícios:** Estabilidade da plataforma, uso justo, segurança adicional.

7.  **Refinamento do Identificador de Recurso (`id`):**
    *   **Proposta:** Mudar `format: byte` para `type: string` (opcionalmente `format: uuid`) para o parâmetro de path `id` em `/device/{id}`.
    *   **Benefícios:** Conformidade, clareza, redução de erros de implementação.

