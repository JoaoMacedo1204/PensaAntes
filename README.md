# Pensa Antes

![Banner do Pensa Antes](docs/images/banner-pensa-antes.png)

**Pense primeiro. Consulte a IA depois.**

O **Pensa Antes** é uma plataforma educacional desenvolvida para estimular o pensamento crítico, a autonomia e a tomada de decisão diante do uso crescente da Inteligência Artificial.

O projeto surgiu da preocupação com a dependência excessiva de ferramentas de IA na resolução de problemas. Sua proposta é incentivar o usuário a **desenvolver e justificar seu próprio raciocínio antes de consultar a Inteligência Artificial**.

Por meio de desafios contextualizados, o usuário analisa informações, constrói hipóteses, consulta opcionalmente a IA, toma uma decisão final e reflete sobre seu desempenho.

O objetivo não é evitar a Inteligência Artificial, mas promover seu uso de maneira consciente, crítica e equilibrada.

**[Acessar a plataforma](https://pensaantes.onrender.com/)**

**[Visualizar apresentação do projeto (PDF)](docs/pensa-antes-pdf.pdf)**

> A aplicação utiliza hospedagem gratuita no Render, portanto o primeiro acesso pode levar alguns instantes.

---

## Funcionalidades

- **Desafios contextualizados:** situações-problema organizadas em diferentes categorias e três níveis de dificuldade (fácil, médio e difícil).
- **Análise inicial independente:** registro do raciocínio e da justificativa do usuário antes de qualquer consulta à IA.
- **Consulta opcional à IA:** integração com o Google Gemini para apresentar uma análise alternativa do desafio.
- **Tomada de decisão:** possibilidade de manter a resposta inicial ou modificá-la com base na análise da IA, quando consultada.
- **Avaliação do pensamento crítico:** avaliação apoiada por IA em cinco critérios — identificação do problema, uso dos dados, formulação de hipóteses, justificativa e pensamento crítico — com pontuação de 0 a 3 por critério, totalizando até 15 pontos.
- **Autoavaliação:** reflexão sobre os pontos positivos e os aspectos que podem ser melhorados.
- **Persistência de dados:** armazenamento de usuários, respostas, decisões e avaliações no PostgreSQL.

## Como funciona

A plataforma incentiva o usuário a desenvolver seu próprio raciocínio antes de consultar a Inteligência Artificial.

O fluxo acontece nas seguintes etapas:

1. **Escolha do desafio:** seleção de uma situação-problema conforme a categoria e o nível de dificuldade.
2. **Análise inicial:** interpretação dos dados disponíveis, elaboração de uma resposta e registro da justificativa.
3. **Consulta opcional à IA:** possibilidade de comparar a análise inicial com uma perspectiva gerada pelo Google Gemini.
4. **Decisão final:** escolha entre manter a resposta inicial ou utilizar a análise da IA, acompanhada de uma justificativa.
5. **Avaliação e autoavaliação:** apresentação de feedback e pontuação, seguida de uma reflexão sobre o próprio desempenho.

## Demonstração

O Pensa Antes está disponível publicamente e pode ser experimentado pelo navegador.

**[Experimentar o Pensa Antes](https://pensaantes.onrender.com/)**

Também foi preparada uma apresentação visual com as principais telas da aplicação, mostrando o funcionamento dos desafios, a consulta opcional à IA e a avaliação final.

**[Ver apresentação completa em PDF](docs/pensa-antes-pdf.pdf)**

## Tecnologias utilizadas

### Backend

- **C# e ASP.NET Core 10:** desenvolvimento da API REST e implementação da lógica de negócio.
- **Entity Framework Core:** mapeamento das entidades e acesso ao banco de dados.
- **Npgsql:** integração entre o Entity Framework Core e o PostgreSQL.

### Frontend

- **HTML5 e CSS3:** estrutura e estilização das interfaces.
- **JavaScript (Vanilla JS):** interatividade, navegação e comunicação com a API.

### Banco de dados

- **PostgreSQL:** persistência dos dados da aplicação.
- **Neon:** hospedagem do banco de dados PostgreSQL na nuvem.

### Inteligência Artificial

- **Google Gemini API:** geração de análises e avaliação das respostas dos usuários.

### Infraestrutura e ferramentas

- **Docker:** empacotamento da aplicação para execução em contêiner.
- **Render:** hospedagem da aplicação e da API.
- **Visual Studio Code:** ambiente de desenvolvimento.
- **Git e GitHub:** versionamento e hospedagem do código-fonte.

---

## Arquitetura e estrutura do projeto

O Pensa Antes utiliza uma **API REST desenvolvida com ASP.NET Core**, responsável pelo processamento das requisições, acesso aos dados e integração com o Google Gemini.

O frontend é desenvolvido com HTML, CSS e JavaScript e disponibilizado pela própria aplicação ASP.NET Core.

### Organização dos diretórios

```text
PensaAntes.API/
├── Controllers/
├── Data/
│   ├── Configurations/
│   ├── AppDbContext.cs
│   └── DBseeder.cs
├── DTOs/
├── Enums/
├── Migrations/
├── Models/
├── Services/
│   ├── Avaliação/
│   └── Gemini/
├── wwwroot/
│   ├── css/
│   ├── fonts/
│   ├── js/
│   └── *.html
├── docs/
│   ├── README.md
│   └── pensa-antes-pdf.pdf
├── Program.cs
└── appsettings.json
```

*Estrutura resumida dos principais diretórios e arquivos do projeto. Os nomes devem corresponder aos arquivos existentes no repositório.*

### Responsabilidades dos componentes

- **Controllers:** recebem as requisições HTTP e disponibilizam os endpoints.
- **DTOs:** definem os dados transferidos entre o frontend e a API.
- **Models:** representam as entidades do domínio.
- **Services:** realizam as integrações com a API do Google Gemini.
- **Data:** contém o contexto do Entity Framework Core, as configurações das entidades e a inicialização dos dados.
- **Migrations:** gerenciam as alterações na estrutura do banco de dados.
- **Enums:** definem os valores utilizados nas regras da aplicação.
- **wwwroot:** reúne os arquivos estáticos do frontend.
- **Program.cs:** configura os serviços e o pipeline HTTP.

### Fluxo de comunicação

1. O usuário interage com a interface web.
2. O JavaScript envia requisições HTTP para a API.
3. Os Controllers processam as requisições e acessam os componentes necessários.
4. O Entity Framework Core realiza as operações no PostgreSQL hospedado no Neon.
5. Quando necessário, os serviços se comunicam com a API do Google Gemini.
6. A API retorna os dados ao frontend, que atualiza a interface conforme a etapa do desafio.

---

## Aprendizados e desafios

O desenvolvimento do Pensa Antes foi uma oportunidade de transformar uma ideia em uma aplicação real, passando pelo planejamento, desenvolvimento, integração de serviços e publicação na internet.

Durante o projeto, aprofundei meus conhecimentos em C#, ASP.NET Core, bancos de dados relacionais e integração com APIs externas. Também tive contato prático com Docker, Render e Neon durante a implantação da aplicação.

Entre os principais desafios estiveram a organização da estrutura do projeto, a integração com o Google Gemini e a configuração do ambiente de produção.

Além do aprendizado técnico, desenvolvi minha capacidade de pesquisar soluções, resolver problemas e aprender com os feedbacks das pessoas que testaram a plataforma.

## Melhorias futuras

O Pensa Antes foi desenvolvido como um **MVP (Produto Mínimo Viável)**, com foco em implementar e validar suas principais funcionalidades.

Entre as possibilidades de evolução estão:

- Implementação de autenticação e gerenciamento de contas.
- Histórico de desafios e acompanhamento do desempenho dos usuários.
- Ampliação das categorias e dos desafios disponíveis.
- Melhorias na acessibilidade e na experiência de navegação.
- Aprimoramento da segurança e do monitoramento da aplicação.

---

## Sobre o desenvolvedor

Projeto desenvolvido por **João Vitor Macedo Gonçalves**, estudante de Análise e Desenvolvimento de Sistemas, como parte do meu aprendizado e desenvolvimento profissional em Engenharia de Software.

O Pensa Antes também foi desenvolvido como meu projeto **Salto**, durante minha participação no programa Carreira de Excelência, do Na Prática.
