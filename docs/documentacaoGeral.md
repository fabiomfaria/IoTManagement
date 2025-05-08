## **Documentação da Solução**

### **Visão Geral**
A solução IoTManagement é composta por múltiplos projetos que implementam uma plataforma para gerenciamento de dispositivos IoT. A arquitetura é baseada em .NET 8 e inclui suporte para Blazor WebAssembly, APIs RESTful e uma API Mock para simulação de dispositivos.

### **Estrutura da Solução**
1. **IoTManagement.API**: API principal para gerenciamento de dispositivos e autenticação.
2. **IoTManagement.Application**: Contém serviços de aplicação e DTOs.
3. **IoTManagement.Domain**: Define entidades, interfaces e exceções.
4. **IoTManagement.Infrastructure**: Implementa repositórios e serviços de infraestrutura.
5. **IoTManagement.UI.Blazor**: Interface do usuário baseada em Blazor WebAssembly.
6. **IoTManagement.MockAPI**: Mock API para simular dispositivos IoT.

---

## **Documentação do Código**

### **IoTManagement.API**
- **Controllers**
  - `DevicesController`: Gerencia dispositivos (listar, obter detalhes, executar comandos).
  - `CommandsController`: Gerencia comandos de dispositivos.
  - `AuthController`: Gerencia autenticação OAuth2.

- **Middlewares**
  - `ExceptionMiddleware`: Manipula exceções globais e retorna respostas padronizadas.

- **Configurações**
  - `SwaggerConfig`: Configura documentação Swagger com suporte a OAuth2.

---

### **IoTManagement.Application**
- **Interfaces**
  - `IDeviceService`: Define operações para gerenciamento de dispositivos.
  - `IAuthService`: Define operações de autenticação.

- **DTOs**
  - `DeviceDto`: Representa um dispositivo.
  - `DeviceCommandDto`: Representa um comando de dispositivo.
  - `OAuth2TokenRequestDto`: Requisição de token OAuth2.

- **Serviços**
  - `DeviceService`: Implementa operações de dispositivos.
  - `AuthService`: Implementa autenticação OAuth2.

---

### **IoTManagement.Domain**
- **Entidades**
  - `Device`: Representa um dispositivo IoT.
  - `DeviceCommand`: Representa um comando configurado.
  - `User`: Representa um usuário.

- **Exceções**
  - `ValidationException`: Erros de validação.
  - `UnauthorizedException`: Erros de autorização.

- **Interfaces**
  - `IDeviceRepository`: Define operações de repositório para dispositivos.
  - `ITokenService`: Define operações para tokens OAuth2.

---

### **IoTManagement.Infrastructure**
- **Repositórios**
  - `CIoTDDeviceRepository`: Implementa repositório de dispositivos.
  - `CIoTDDeviceCommandRepository`: Implementa repositório de comandos.

- **Serviços**
  - `TelnetClient`: Implementa comunicação Telnet.
  - `TokenService`: Gerencia tokens OAuth2.

---

### **IoTManagement.UI.Blazor**
- **Páginas**
  - `Login.razor`: Página de login.
  
- **Serviços**
  - `DeviceService`: Consome APIs de dispositivos.
  - `AuthenticationService`: Gerencia autenticação no cliente.

---

### **IoTManagement.MockAPI**
- **Endpoints**
  - `GET /device`: Lista dispositivos.
  - `POST /device`: Adiciona um dispositivo.
  - `GET /device/{id}`: Obtém detalhes de um dispositivo.

- **MockData**
  - Simula dispositivos e comandos para testes.
