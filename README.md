# Instruções

- Caso o tempo não seja suficiente, priorize a **qualidade, o padrão e a estrutura do código**, definindo claramente quais funcionalidades não serão implementadas.
- Caso alguma funcionalidade não seja implementada, isso **deve ser documentado neste README**, explicando o motivo.
- O código fornecido contém **problemas que devem ser identificados e corrigidos**.
- Fique a vontade para criar, renomear e remover pastas,bibliotecas e até a solução não utilizadas.
- O sistema deve **compilar corretamente e executar todas as ações previstas**.
- O código final **não deve apresentar erros nem warnings** durante a compilação.
- Deve ser enviado via e-mail para consultoria com o link do projeto no Github. A consultaoria terá até terça-feira dia 13 as as 13 horas para encaminhar o e-mail.

---

## 1. Introdução

Sistema para um prestador de serviços (ou pequena equipe) registrar clientes, abrir ordens de serviço, acompanhar status, registrar valores e anexar fotos de antes/depois do serviço.

---

## 2. Funcionalidades Detalhadas

### 2.1 Cadastro de Cliente

#### Objetivo
Permitir registrar e consultar dados do cliente para vinculação em Ordens de Serviço (OS).

#### Campos (mínimo)
- Nome (obrigatório, 2–150 caracteres)
- Id (gerado pelo sistema)
- Telefone (opcional, até 30 caracteres)
- E-mail (opcional, até 120 caracteres, formato válido)
- Documento (CPF/CNPJ) (opcional, até 30 caracteres, sem validação pesada)
- Data de criação (gerado pelo sistema)

#### Regras de Negócio
1. Nome é obrigatório e não pode conter apenas whitespace.
2. Telefone e e-mail podem ser nulos; se informados, devem ser trimados.
3. Opcionalmente, bloquear ou alertar duplicidade por:
   - Documento (CPF/CNPJ), quando informado
   - Telefone, quando informado

#### Operações
- Criar cliente
- Consultar cliente por Id
- Buscar cliente por telefone ou documento

#### Casos de Teste
- Criar cliente com nome válido retorna 201 Created + id
- Criar cliente sem nome retorna 400 Validation Error
- Criar cliente com e-mail inválido retorna 400 Validation Error
- Criar cliente com telefone e buscar retorna dados consistentes
- Criar cliente com documento duplicado (se regra ativa) retorna 409 Conflict ou 400

---

### 2.2 Abertura de Ordem de Serviço

#### Objetivo
Criar uma OS vinculada a um cliente, com descrição e dados iniciais.

#### Campos (mínimo)
- ClienteId (obrigatório)
- Descrição do serviço (obrigatório, 1–500 caracteres)
- Número da OS (gerado automaticamente, sequencial/identity)
- Status (inicial = Aberta)
- Data de abertura (gerado pelo sistema)
- Valor do serviço (decimal(18,2)) (opcional no momento da abertura)
- Moeda (BRL)
- Data de atualização valor (opcional)

#### Regras de Negócio
1. Só é possível abrir OS para cliente existente.
2. Descrição é obrigatória.
3. Status inicial deve ser sempre Aberta.
4. Número da OS deve ser único e sequencial.
5. Regra de negócio item 2.4 

#### Operações
- Abrir OS
- Consultar OS por Id
- Listar OS por cliente, status ou período

#### Casos de Teste
- Abrir OS para cliente existente retorna 201 Created
- Abrir OS para cliente inexistente retorna 404 Not Found
- Abrir OS com descrição vazia retorna 400 Bad Request
- Consultar OS recém-criada retorna status Aberta

---

### 2.3 Status da Ordem de Serviço

#### Objetivo
Permitir acompanhar o ciclo do serviço.

#### Estados
- Aberta
- Em Execução
- Finalizada

#### Regras de Transição
- Aberta -> Em Execução (permitido)
- Em Execução -> Finalizada (permitido)
- Aberta -> Finalizada (bloqueado)
- Finalizada -> qualquer outro (bloqueado)

#### Operações
- Alterar status
- Registrar datas opcionais:
  - StartedAt ao entrar em Em Execução
  - FinishedAt ao entrar em Finalizada

#### Casos de Teste
- Alterar Aberta para Em Execução retorna 200 OK
- Alterar Em Execução para Finalizada retorna 200 OK
- Alterar Finalizada para outro status retorna 409 Conflict

---

### 2.4 Valor do Serviço

#### Objetivo
Permitir definir ou ajustar o valor do serviço.

#### Campos
- Valor (decimal(18,2))
- Moeda (BRL)
- Data de atualização (opcional)

#### Regras de Negócio
1. Valor pode ser nulo enquanto Aberta ou Em Execução.
2. Valor pode ser obrigatório para finalizar a OS.
3. Valor não pode ser negativo.
4. Após Finalizada, não permitir alteração.

#### Operações
- Definir ou alterar valor
- Validar valor ao finalizar OS

---

### 2.5 Fotos Antes / Depois (Opcional)

#### Objetivo
Permitir anexar evidências do serviço.

#### Campos do Anexo
- Id
- ServiceOrderId
- Type (Before | After)
- FileName
- ContentType (image/jpeg, image/png)
- SizeBytes
- StoragePath
- UploadedAt

#### Regras de Negócio
1. Aceitar apenas JPG e PNG.
2. Tamanho máximo sugerido: 5MB.
3. Permitir múltiplos anexos.
4. Upload local em /data/uploads (container ou volume).

---

## 3. API Sugerida

### Clientes
- POST /v1/customers
- GET /v1/customers/{id}

### Ordens de Serviço
- POST /v1/service-orders
- GET /v1/service-orders/{id}
- PATCH /v1/service-orders/{id}/status
- PUT /v1/service-orders/{id}/price
- POST /v1/service-orders/{id}/attachments/before
- POST /v1/service-orders/{id}/attachments/after
- GET /v1/service-orders/{id}/attachments

---

## 4. Requisitos Não Funcionais (Opcional)

### Performance
- Upload deve ser feito via streaming, evitando carregar todo o arquivo em memória.

### Segurança
- Validar content-type e extensão real do arquivo.
- Sanitizar nome do arquivo.

### Observabilidade
- Registrar logs para criação de cliente, abertura de OS e mudança de status.
