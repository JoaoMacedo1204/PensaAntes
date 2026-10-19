
using Microsoft.EntityFrameworkCore;
using PensaAntes.Api.Enums;
using PensaAntes.Api.Models;

namespace PensaAntes.Api.Data;

public static class DbSeeder
{
    private const string NomeCategoriaMarketing = "Marketing / Dados";
    private const string NomeCategoriaTecnologia = "Tecnologia";

    private const string TituloMarketingMedio = "Uma campanha que não está funcionando";

    private const string TituloMarketingFacil = "Qual campanha merece continuar?";

    private const string TituloMarketingDificil = "Por que as vendas estão caindo?";

    private const string TituloTecnologiaFacil = "Qual computador escolher?";

    private const string TituloTecnologiaMedio = "O aplicativo está ficando lento";

    private const string TituloTecnologiaDificil = "Segurança ou experiência do usuário?";
   
    private const string NomeCategoriaCotidiano = "Cotidiano";

    private const string TituloCotidianoFacil = "Qual caminho vale mais a pena?";

    private const string TituloCotidianoMedio = "Por que estou ficando sem tempo?";

    private const string TituloCotidianoDificil = "Uma decisão que afeta toda a comunidade";
    
    private const string NomeCategoriaMeioAmbiente = "Meio Ambiente";

    private const string TituloMeioAmbienteFacil = "Como economizar água em casa?";

    private const string TituloMeioAmbienteMedio = "O lixo da escola está aumentando";

    private const string TituloMeioAmbienteDificil = "Como reduzir a poluição da cidade?";

    private const string NomeCategoriaFinancas = "Finanças";

    private const string TituloFinancasFacil = "Comprar agora ou esperar?";

    private const string TituloFinancasMedio = "Para onde está indo meu dinheiro?";

    private const string TituloFinancasDificil = "Crescer ou proteger o negócio?";

    public static async Task SeedAsync(AppDbContext context)
    {
        // =====================================================
        // 1. CATEGORIA: MARKETING / DADOS
        // =====================================================

        var categoriaMarketing = await context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Nome == NomeCategoriaMarketing);

        if (categoriaMarketing is null)
        {
            categoriaMarketing = new Categoria
            {
                Nome = NomeCategoriaMarketing,
                Descricao =
                    "Desafios relacionados à análise de dados e tomada de decisões em contextos de marketing."
            };

            context.Categorias.Add(categoriaMarketing);
        }

        // -----------------------------------------------------
        // MARKETING / DADOS — MÉDIO
        // -----------------------------------------------------

        var marketingMedioExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloMarketingMedio);

        if (!marketingMedioExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMarketingMedio,

                Descricao =
                    "Analisar os resultados de uma campanha digital e identificar por que o aumento de investimento não gerou crescimento proporcional nas vendas.",

                SituacaoProblema =
                    "Uma empresa aumentou o investimento em uma campanha digital, mas as vendas não cresceram conforme o esperado.",

                DadosDisponiveis = string.Join("\n", new[]
                {
                    "Investimento antes: R$20.000",
                    "Investimento depois: R$35.000",
                    "Visitas antes: 50.000",
                    "Visitas depois: 80.000",
                    "Conversão antes: 4,0%",
                    "Conversão depois: 2,5%",
                    "Vendas antes: 2.000",
                    "Vendas depois: 2.000",
                    "Ticket médio antes: R$100",
                    "Ticket médio depois: R$115"
                }),

                Objetivo =
                    "Identificar o principal problema apresentado pelos dados e propor uma decisão.",

                PerguntaPrincipal =
                    "Com base nos dados apresentados, qual decisão você tomaria e por quê?",

                Dificuldade = NivelDificuldade.Medio,
                Categoria = categoriaMarketing
            });
        }

        // -----------------------------------------------------
        // MARKETING / DADOS — FÁCIL
        // -----------------------------------------------------

        const string dadosMarketingFacil = """
            CAMPANHA A
            Investimento: R$400
            Pessoas alcançadas: 8.000
            Cliques: 400
            Compras: 20
            Receita gerada: R$1.200

            CAMPANHA B
            Investimento: R$600
            Pessoas alcançadas: 12.000
            Cliques: 600
            Compras: 24
            Receita gerada: R$1.440
            """;

        var marketingFacil = await context.Desafios
            .FirstOrDefaultAsync(d =>
                d.Titulo == TituloMarketingFacil);

        if (marketingFacil is null)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMarketingFacil,

                Descricao =
                    "Comparar os resultados de duas campanhas de marketing e decidir qual delas merece continuar recebendo investimento.",

                SituacaoProblema =
                    "Uma loja de roupas tem orçamento limitado e só consegue continuar investindo em uma das duas campanhas de marketing. O gerente pediu sua ajuda para analisar os resultados e recomendar qual campanha manter.",

                DadosDisponiveis = dadosMarketingFacil,

                Objetivo =
                    "Identificar qual campanha apresenta o melhor resultado para a loja, considerando os custos e as vendas.",

                PerguntaPrincipal =
                    "Qual campanha você recomendaria manter e quais dados justificam sua escolha?",

                Dificuldade = NivelDificuldade.Facil,
                Categoria = categoriaMarketing
            });
        }
        else
        {
            // Preserva o desafio existente e atualiza apenas os dados.
            marketingFacil.DadosDisponiveis = dadosMarketingFacil;
        }

        // -----------------------------------------------------
        // MARKETING / DADOS — DIFÍCIL
        // -----------------------------------------------------

        const string dadosMarketingDificil = """
            JULHO
            Investimento em anúncios: R$10.000
            Visitantes do site: 20.000
            Pedidos realizados: 800
            Ticket médio: R$150
            Abandono de carrinho: 55%
            Reclamações sobre entrega: 20
            Prazo médio de entrega: 4 dias

            AGOSTO
            Investimento em anúncios: R$14.000
            Visitantes do site: 28.000
            Pedidos realizados: 840
            Ticket médio: R$145
            Abandono de carrinho: 62%
            Reclamações sobre entrega: 38

            SETEMBRO
            Investimento em anúncios: R$18.000
            Visitantes do site: 36.000
            Pedidos realizados: 720
            Ticket médio: R$140
            Abandono de carrinho: 72%
            Reclamações sobre entrega: 75
            Prazo médio de entrega: 8 dias
            """;

        var marketingDificil = await context.Desafios
            .FirstOrDefaultAsync(d =>
                d.Titulo == TituloMarketingDificil);

        if (marketingDificil is null)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMarketingDificil,

                Descricao =
                    "Investigar por que uma loja virtual está vendendo menos apesar do aumento dos investimentos em marketing.",

                SituacaoProblema =
                    "Uma loja virtual investiu mais em publicidade nos últimos três meses para aumentar suas vendas. Entretanto, apesar do crescimento no número de visitantes, a receita caiu. A liderança precisa decidir se deve aumentar os anúncios, melhorar a experiência de compra ou revisar sua estratégia comercial.",

                DadosDisponiveis = dadosMarketingDificil,

                Objetivo =
                    "Identificar possíveis causas da queda de desempenho, priorizar ações e propor indicadores para acompanhar os resultados.",

                PerguntaPrincipal =
                    "Diante desses dados, quais hipóteses explicam a queda nas vendas, quais ações você priorizaria e como verificaria se suas decisões funcionaram?",

                Dificuldade = NivelDificuldade.Dificil,
                Categoria = categoriaMarketing
            });
        }
        else
        {
            marketingDificil.DadosDisponiveis = dadosMarketingDificil;
        }

        // =====================================================
        // 2. CATEGORIA: TECNOLOGIA
        // =====================================================

        var categoriaTecnologia = await context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Nome == NomeCategoriaTecnologia);

        if (categoriaTecnologia is null)
        {
            categoriaTecnologia = new Categoria
            {
                Nome = NomeCategoriaTecnologia,

                Descricao =
                    "Desafios sobre o uso da tecnologia, resolução de problemas e tomada de decisões envolvendo sistemas e segurança digital."
            };

            context.Categorias.Add(categoriaTecnologia);
        }

        // -----------------------------------------------------
        // TECNOLOGIA — FÁCIL
        // -----------------------------------------------------

        var tecnologiaFacilExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloTecnologiaFacil);

        if (!tecnologiaFacilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloTecnologiaFacil,

                Descricao =
                    "Ajudar uma escola a escolher computadores, comparando preços, características e necessidades dos alunos.",

                SituacaoProblema =
                    "Uma escola quer comprar 10 computadores para seu laboratório de informática. Os alunos vão usá-los para fazer pesquisas, trabalhos escolares e participar de aulas online. A escola encontrou dois modelos, mas precisa decidir qual deles vale mais a pena comprar.",

                DadosDisponiveis = """
                    MODELO A
                    Preço por computador: R$2.000
                    Memória RAM: 8 GB
                    Armazenamento: SSD de 256 GB
                    Garantia: 1 ano
                    Preço de 10 computadores: R$20.000

                    MODELO B
                    Preço por computador: R$2.500
                    Memória RAM: 16 GB
                    Armazenamento: SSD de 512 GB
                    Garantia: 3 anos
                    Preço de 10 computadores: R$25.000
                    """,

                Objetivo =
                    "Escolher o computador que melhor atende às necessidades da escola, considerando o preço e as características de cada modelo.",

                PerguntaPrincipal =
                    "Qual dos dois modelos você escolheria para a escola? Explique sua decisão usando os dados apresentados.",

                Dificuldade = NivelDificuldade.Facil,
                Categoria = categoriaTecnologia
            });
        }

        // -----------------------------------------------------
        // TECNOLOGIA — MÉDIO
        // -----------------------------------------------------

        var tecnologiaMedioExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloTecnologiaMedio);

        if (!tecnologiaMedioExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloTecnologiaMedio,

                Descricao =
                    "Investigar por que um aplicativo está ficando lento e sugerir maneiras de melhorar seu funcionamento.",

                SituacaoProblema =
                    "Um aplicativo de entregas começou a receber muitas reclamações porque está demorando para carregar as informações. Nos últimos meses, a quantidade de pessoas usando o aplicativo aumentou bastante. Ao mesmo tempo, ele ficou mais lento e começou a apresentar mais erros. A equipe responsável precisa descobrir o que pode estar causando esses problemas e decidir por onde começar a resolvê-los.",

                DadosDisponiveis = """
                    ANTES
                    Usuários por dia: 5.000
                    Tempo médio de resposta: 1,2 segundo
                    Uso do processador do servidor: 45%
                    Requisições com erro: 1%
                    Reclamações por semana: 20

                    AGORA
                    Usuários por dia: 12.000
                    Tempo médio de resposta: 4,5 segundos
                    Uso do processador do servidor: 92%
                    Requisições com erro: 8%
                    Reclamações por semana: 140

                    INFORMAÇÃO IMPORTANTE
                    O servidor é o computador que processa as informações do aplicativo. Quanto maior o uso do processador, mais ocupado ele está.
                    """,

                Objetivo =
                    "Analisar as mudanças no funcionamento do aplicativo, identificar possíveis causas da lentidão e sugerir melhorias.",

                PerguntaPrincipal =
                    "O que pode estar deixando o aplicativo lento? Qual problema você investigaria primeiro e como saberia se sua solução deu certo?",

                Dificuldade = NivelDificuldade.Medio,
                Categoria = categoriaTecnologia
            });
        }

        // -----------------------------------------------------
        // TECNOLOGIA — DIFÍCIL
        // -----------------------------------------------------

        var tecnologiaDificilExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloTecnologiaDificil);

        if (!tecnologiaDificilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloTecnologiaDificil,

                Descricao =
                    "Ajudar uma plataforma financeira a proteger seus usuários sem tornar o acesso ao aplicativo muito complicado.",

                SituacaoProblema =
                    "Uma plataforma financeira percebeu que algumas pessoas estão tentando acessar contas de outros usuários sem autorização. Para aumentar a segurança, a equipe sugeriu adicionar uma etapa de verificação durante o login, como um código enviado ao celular. Porém, em um teste, a empresa percebeu que mais pessoas desistiram de entrar no aplicativo porque o processo ficou mais demorado. Agora, a empresa precisa decidir como melhorar a segurança sem prejudicar tanto a experiência dos usuários.",

                DadosDisponiveis = """
                    SITUAÇÃO ATUAL
                    Logins realizados no mês: 100.000
                    Tentativas de acesso suspeitas: 2.000
                    Acessos indevidos confirmados: 40
                    Usuários que desistem durante o login: 3%
                    Reclamações sobre acesso: 100

                    TESTE COM VERIFICAÇÃO ADICIONAL
                    Logins realizados no teste: 10.000
                    Tentativas de acesso suspeitas: 200
                    Acessos indevidos confirmados: 1
                    Usuários que desistem durante o login: 11%
                    Reclamações sobre acesso: 35
                    Custo estimado para implementar a solução: R$80.000

                    INFORMAÇÃO IMPORTANTE
                    O teste envolveu menos logins do que a situação atual. Por isso, é importante considerar essa diferença ao comparar os resultados.
                    """,

                Objetivo =
                    "Propor uma solução que ajude a proteger as contas dos usuários, considerando também a facilidade de acesso e os custos para a empresa.",

                PerguntaPrincipal =
                    "Você exigiria essa verificação de todos os usuários, apenas em acessos suspeitos ou escolheria outra solução? Explique sua decisão e como avaliaria se ela funcionou.",

                Dificuldade = NivelDificuldade.Dificil,
                Categoria = categoriaTecnologia
            });
        }

        // =====================================================
        // 3. CATEGORIA: COTIDIANO
        // =====================================================

        var categoriaCotidiano = await context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Nome == NomeCategoriaCotidiano);

        if (categoriaCotidiano is null)
        {
            categoriaCotidiano = new Categoria
            {
                Nome = NomeCategoriaCotidiano,
                Descricao =
                    "Desafios sobre situações do dia a dia, organização pessoal e decisões que envolvem diferentes necessidades e prioridades."
            };

            context.Categorias.Add(categoriaCotidiano);
        }

        // -----------------------------------------------------
        // COTIDIANO — FÁCIL
        // -----------------------------------------------------

        var cotidianoFacilExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloCotidianoFacil);

        if (!cotidianoFacilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloCotidianoFacil,

                Descricao =
                    "Comparar duas opções de transporte para escolher a mais adequada a uma rotina de estudos.",

                SituacaoProblema =
                    "Uma estudante precisa ir à faculdade cinco dias por semana. Ela encontrou duas opções de transporte: ônibus ou aplicativo de corrida. O ônibus é mais barato, mas demora mais. Já o aplicativo é mais rápido, porém custa mais caro. Ela precisa escolher como vai se deslocar, considerando seu orçamento e o tempo disponível.",

                DadosDisponiveis = """
                    ÔNIBUS
                    Custo por viagem: R$5
                    Tempo por viagem: 50 minutos
                    Viagens por dia: 2
                    Dias de aula por semana: 5

                    APLICATIVO DE CORRIDA
                    Custo por viagem: R$18
                    Tempo por viagem: 25 minutos
                    Viagens por dia: 2
                    Dias de aula por semana: 5

                    INFORMAÇÃO IMPORTANTE
                    A estudante tem R$200 por semana disponíveis para transporte.
                    """,

                Objetivo =
                    "Escolher a opção de transporte mais adequada, considerando os custos, o tempo de deslocamento e o orçamento semanal.",

                PerguntaPrincipal =
                    "Qual transporte você recomendaria? Explique sua escolha usando os dados apresentados.",

                Dificuldade = NivelDificuldade.Facil,
                Categoria = categoriaCotidiano
            });
        }

        // -----------------------------------------------------
        // COTIDIANO — MÉDIO
        // -----------------------------------------------------

        var cotidianoMedioExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloCotidianoMedio);

        if (!cotidianoMedioExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloCotidianoMedio,

                Descricao =
                    "Analisar uma rotina semanal e identificar mudanças que ajudem a equilibrar estudos, descanso e atividades pessoais.",

                SituacaoProblema =
                    "Uma pessoa trabalha durante o dia e estuda à noite. Nas últimas semanas, começou a sentir muito cansaço e percebeu que está deixando algumas tarefas importantes para depois. Ela decidiu anotar como utiliza seu tempo para entender o que pode estar acontecendo. Agora, precisa reorganizar sua rotina sem prejudicar o trabalho, os estudos e a saúde.",

                DadosDisponiveis = """
                    ROTINA DE SEGUNDA A SEXTA
                    Trabalho: 6 horas por dia
                    Faculdade: 4 horas por dia
                    Transporte: 2 horas por dia
                    Sono: 6 horas por noite
                    Redes sociais e entretenimento: 3 horas por dia
                    Alimentação e cuidados pessoais: 2 horas por dia
                    Outras atividades: 1 hora por dia

                    INFORMAÇÕES ADICIONAIS
                    Tarefas da faculdade atrasadas: 4
                    Dias com sensação de cansaço na semana: 5
                    Tempo de estudo fora das aulas: 0 hora por dia
                    Meta de sono: 8 horas por noite
                    """,

                Objetivo =
                    "Identificar os principais problemas na organização do tempo e propor mudanças realistas para melhorar a rotina.",

                PerguntaPrincipal =
                    "O que você mudaria nessa rotina primeiro? Como organizaria o tempo para melhorar o descanso e conseguir realizar as tarefas?",

                Dificuldade = NivelDificuldade.Medio,
                Categoria = categoriaCotidiano
            });
        }

        // -----------------------------------------------------
        // COTIDIANO — DIFÍCIL
        // -----------------------------------------------------

        var cotidianoDificilExiste = await context.Desafios
            .AnyAsync(d =>
                d.Titulo == TituloCotidianoDificil);

        if (!cotidianoDificilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloCotidianoDificil,

                Descricao =
                    "Analisar diferentes necessidades de uma comunidade e decidir como utilizar um orçamento limitado.",

                SituacaoProblema =
                    "Um bairro recebeu R$100.000 para realizar melhorias. Os moradores apresentaram três problemas importantes: uma praça abandonada, ruas com pouca iluminação e pontos de ônibus sem cobertura. A prefeitura informou que o dinheiro não será suficiente para resolver tudo de uma vez. A comunidade precisa decidir quais melhorias devem ser realizadas primeiro, considerando custos, segurança e quantidade de pessoas beneficiadas.",

                DadosDisponiveis = """
                    OPÇÃO A — REFORMAR A PRAÇA
                    Custo: R$70.000
                    Moradores beneficiados: aproximadamente 800
                    Problema: falta de espaços seguros para lazer
                    Prazo de execução: 60 dias

                    OPÇÃO B — MELHORAR A ILUMINAÇÃO
                    Custo: R$60.000
                    Moradores beneficiados: aproximadamente 1.500
                    Problema: ruas escuras e sensação de insegurança
                    Prazo de execução: 30 dias

                    OPÇÃO C — INSTALAR COBERTURAS NOS PONTOS DE ÔNIBUS
                    Custo: R$40.000
                    Moradores beneficiados: aproximadamente 900
                    Problema: exposição à chuva e ao sol
                    Prazo de execução: 20 dias

                    ORÇAMENTO TOTAL
                    R$100.000

                    INFORMAÇÃO IMPORTANTE
                    As estimativas de moradores beneficiados podem incluir pessoas que utilizam mais de um desses espaços.
                    """,

                Objetivo =
                    "Definir quais melhorias devem receber prioridade, considerando as necessidades da comunidade, os custos e os benefícios esperados.",

                PerguntaPrincipal =
                    "Como você utilizaria os R$100.000? Quais melhorias escolheria primeiro, por quê e como avaliaria se a decisão trouxe benefícios para os moradores?",

                Dificuldade = NivelDificuldade.Dificil,
                Categoria = categoriaCotidiano
            });
        }

        // =====================================================
        // 4. CATEGORIA: MEIO AMBIENTE
        // =====================================================

        var categoriaMeioAmbiente = await context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Nome == NomeCategoriaMeioAmbiente);

        if (categoriaMeioAmbiente is null)
        {
            categoriaMeioAmbiente = new Categoria
            {
                Nome = NomeCategoriaMeioAmbiente,
                Descricao =
                    "Desafios sobre consumo consciente, redução de desperdícios e decisões para preservar o meio ambiente."
            };

            context.Categorias.Add(categoriaMeioAmbiente);
        }

        // -----------------------------------------------------
        // MEIO AMBIENTE — FÁCIL
        // -----------------------------------------------------

        var meioAmbienteFacilExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloMeioAmbienteFacil);

        if (!meioAmbienteFacilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMeioAmbienteFacil,

                Descricao =
                    "Comparar hábitos de consumo de água e escolher mudanças que ajudem a evitar desperdícios.",

                SituacaoProblema =
                    "Uma família de quatro pessoas percebeu que sua conta de água aumentou nos últimos meses. Para tentar economizar, a família decidiu observar dois hábitos do dia a dia: o tempo gasto no banho e o uso da mangueira para lavar o quintal. Agora, precisa decidir quais mudanças podem ajudar a reduzir o consumo de água.",

                DadosDisponiveis = """
                    BANHOS
                    Pessoas na casa: 4
                    Banhos por pessoa: 1 por dia
                    Duração média de cada banho: 15 minutos
                    Consumo estimado do chuveiro: 9 litros por minuto

                    LIMPEZA DO QUINTAL
                    Frequência: 2 vezes por semana
                    Tempo de uso da mangueira: 20 minutos por limpeza
                    Consumo estimado: 12 litros por minuto

                    POSSÍVEIS MUDANÇAS
                    Opção A: reduzir cada banho para 10 minutos
                    Opção B: reduzir o uso da mangueira para 10 minutos por limpeza
                    """,

                Objetivo =
                    "Comparar o consumo de água nas duas atividades e identificar qual mudança pode gerar maior economia.",

                PerguntaPrincipal =
                    "Qual mudança você recomendaria fazer primeiro? Explique sua escolha usando os dados apresentados.",

                Dificuldade = NivelDificuldade.Facil,
                Categoria = categoriaMeioAmbiente
            });
        }

        // -----------------------------------------------------
        // MEIO AMBIENTE — MÉDIO
        // -----------------------------------------------------

        var meioAmbienteMedioExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloMeioAmbienteMedio);

        if (!meioAmbienteMedioExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMeioAmbienteMedio,

                Descricao =
                    "Investigar por que uma escola está produzindo mais lixo e propor ações para reduzir o desperdício.",

                SituacaoProblema =
                    "Uma escola percebeu que a quantidade de lixo produzido pelos alunos aumentou bastante nos últimos meses. A direção acredita que parte desse aumento está relacionada ao uso de embalagens descartáveis e ao desperdício de alimentos durante o intervalo. Para entender melhor a situação, a escola começou a registrar os tipos de resíduos produzidos. Agora, precisa decidir quais ações devem ser realizadas primeiro.",

                DadosDisponiveis = """
                    QUANTIDADE DE LIXO POR SEMANA

                    ANTES
                    Plásticos: 40 kg
                    Papel: 30 kg
                    Restos de alimentos: 50 kg
                    Outros resíduos: 20 kg
                    Total: 140 kg

                    AGORA
                    Plásticos: 75 kg
                    Papel: 35 kg
                    Restos de alimentos: 80 kg
                    Outros resíduos: 25 kg
                    Total: 215 kg

                    INFORMAÇÕES ADICIONAIS
                    Alunos matriculados antes: 500
                    Alunos matriculados agora: 650
                    Lixeiras para coleta seletiva: 4
                    Campanhas de conscientização realizadas: nenhuma
                    """,

                Objetivo =
                    "Identificar os principais fatores relacionados ao aumento do lixo e propor ações para diminuir a quantidade de resíduos produzidos.",

                PerguntaPrincipal =
                    "Quais tipos de lixo merecem mais atenção? Que ações você colocaria em prática primeiro e como verificaria se elas estão funcionando?",

                Dificuldade = NivelDificuldade.Medio,
                Categoria = categoriaMeioAmbiente
            });
        }

        // -----------------------------------------------------
        // MEIO AMBIENTE — DIFÍCIL
        // -----------------------------------------------------

        var meioAmbienteDificilExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloMeioAmbienteDificil);

        if (!meioAmbienteDificilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloMeioAmbienteDificil,

                Descricao =
                    "Escolher investimentos para melhorar a qualidade do ar de uma cidade, considerando custos, benefícios e limitações do orçamento.",

                SituacaoProblema =
                    "Uma cidade está enfrentando problemas com a poluição do ar. Muitos moradores reclamam da fumaça dos veículos e da falta de áreas verdes. A prefeitura recebeu R$2 milhões para investir em melhorias ambientais, mas esse dinheiro não é suficiente para realizar todas as propostas. A equipe responsável precisa escolher quais projetos devem receber prioridade, considerando os custos, os benefícios esperados e o tempo necessário para apresentar resultados.",

                DadosDisponiveis = """
                    PROJETO A — ÔNIBUS MENOS POLUENTES
                    Custo: R$1.500.000
                    Redução estimada de emissões: 18%
                    Pessoas beneficiadas: aproximadamente 20.000
                    Prazo de implantação: 12 meses

                    PROJETO B — PLANTIO DE ÁRVORES
                    Custo: R$600.000
                    Árvores previstas: 3.000
                    Pessoas beneficiadas: aproximadamente 12.000
                    Prazo de implantação: 6 meses
                    Benefícios ambientais: aumentam ao longo dos anos

                    PROJETO C — CICLOVIAS
                    Custo: R$900.000
                    Extensão prevista: 8 km
                    Pessoas beneficiadas: aproximadamente 8.000
                    Prazo de implantação: 9 meses
                    Redução estimada de viagens de carro: 5% nas regiões atendidas

                    ORÇAMENTO DISPONÍVEL
                    R$2.000.000

                    INFORMAÇÃO IMPORTANTE
                    Os benefícios são estimativas.
                    A redução de emissões do Projeto A e a redução de viagens do Projeto C são medidas diferentes e não devem ser somadas diretamente.
                    Também pode haver pessoas beneficiadas por mais de um projeto.
                    """,

                Objetivo =
                    "Escolher uma estratégia para melhorar a qualidade do ar, considerando o orçamento, os benefícios ambientais e as necessidades da população.",

                PerguntaPrincipal =
                    "Como você investiria os R$2 milhões? Quais projetos escolheria, por que daria prioridade a eles e como acompanharia os resultados?",

                Dificuldade = NivelDificuldade.Dificil,
                Categoria = categoriaMeioAmbiente
            });
        }

        // =====================================================
        // 5. CATEGORIA: FINANÇAS
        // =====================================================

        var categoriaFinancas = await context.Categorias
            .FirstOrDefaultAsync(c =>
                c.Nome == NomeCategoriaFinancas);

        if (categoriaFinancas is null)
        {
            categoriaFinancas = new Categoria
            {
                Nome = NomeCategoriaFinancas,
                Descricao =
                    "Desafios sobre planejamento financeiro, controle de gastos, investimentos e decisões que envolvem riscos e prioridades."
            };

            context.Categorias.Add(categoriaFinancas);
        }

        // -----------------------------------------------------
        // FINANÇAS — FÁCIL
        // -----------------------------------------------------

        var financasFacilExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloFinancasFacil);

        if (!financasFacilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloFinancasFacil,

                Descricao =
                    "Comparar formas de pagamento e decidir como realizar uma compra sem comprometer o orçamento.",

                SituacaoProblema =
                    "Uma pessoa quer comprar um celular novo porque o aparelho atual está ficando lento, mas ainda funciona. Ela encontrou uma promoção e está pensando em aproveitar a oferta. Porém, também precisa guardar dinheiro para outras despesas. Agora, precisa decidir se vale a pena comprar o celular imediatamente ou esperar mais um pouco.",

                DadosDisponiveis = """
                    SITUAÇÃO FINANCEIRA
                    Dinheiro guardado: R$1.500
                    Renda mensal: R$2.000
                    Despesas mensais: R$1.600
                    Valor que sobra por mês: R$400

                    OPÇÕES DE COMPRA
                    Preço à vista: R$1.200
                    Preço parcelado: 6 parcelas de R$230
                    Total parcelado: R$1.380

                    INFORMAÇÃO IMPORTANTE
                    A pessoa quer manter pelo menos R$1.000 guardados para emergências.
                    """,

                Objetivo =
                    "Avaliar as opções de compra considerando o preço, a renda disponível e a importância de manter dinheiro para emergências.",

                PerguntaPrincipal =
                    "Você compraria o celular à vista, parcelado ou esperaria mais um pouco? Explique sua escolha usando os dados apresentados.",

                Dificuldade = NivelDificuldade.Facil,
                Categoria = categoriaFinancas
            });
        }

        // -----------------------------------------------------
        // FINANÇAS — MÉDIO
        // -----------------------------------------------------

        var financasMedioExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloFinancasMedio);

        if (!financasMedioExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloFinancasMedio,

                Descricao =
                    "Analisar um orçamento mensal, identificar gastos que podem ser ajustados e propor um plano para economizar.",

                SituacaoProblema =
                    "Uma pessoa começou a trabalhar recentemente e percebeu que, mesmo recebendo seu salário todos os meses, quase nunca consegue guardar dinheiro. Para entender o problema, decidiu anotar seus gastos durante um mês. Seu objetivo é começar a economizar, mas sem deixar de pagar as contas importantes ou abandonar completamente os momentos de lazer.",

                DadosDisponiveis = """
                    RENDA MENSAL
                    R$3.000

                    DESPESAS MENSAIS
                    Moradia: R$900
                    Alimentação: R$550
                    Transporte: R$300
                    Contas (água, luz, internet): R$250
                    Lazer: R$450
                    Compras pessoais: R$400
                    Assinaturas: R$150
                    Total: R$3.000

                    META FINANCEIRA
                    Valor que deseja guardar por mês: R$500
                    Prazo da meta: 6 meses
                    Objetivo: formar uma reserva de R$3.000
                    """,

                Objetivo =
                    "Identificar quais gastos podem ser ajustados e criar um plano realista para guardar R$500 por mês.",

                PerguntaPrincipal =
                    "Quais despesas você reduziria para alcançar a meta de economia? Como organizaria o orçamento sem prejudicar as necessidades básicas?",

                Dificuldade = NivelDificuldade.Medio,
                Categoria = categoriaFinancas
            });
        }

        // -----------------------------------------------------
        // FINANÇAS — DIFÍCIL
        // -----------------------------------------------------

        var financasDificilExiste = await context.Desafios
            .AnyAsync(d => d.Titulo == TituloFinancasDificil);

        if (!financasDificilExiste)
        {
            context.Desafios.Add(new Desafio
            {
                Titulo = TituloFinancasDificil,

                Descricao =
                    "Decidir como uma pequena empresa deve utilizar seu dinheiro, equilibrando crescimento, segurança financeira e riscos.",

                SituacaoProblema =
                    "Uma pequena loja de roupas está vendendo bem e conseguiu guardar dinheiro nos últimos meses. A dona da loja quer aproveitar esse momento para fazer o negócio crescer. Porém, ela também está preocupada com possíveis quedas nas vendas e despesas inesperadas. A empresa tem três possibilidades: investir em publicidade, comprar novos equipamentos ou guardar parte do dinheiro para emergências. Como os recursos são limitados, ela precisa decidir como distribuir o valor disponível.",

                DadosDisponiveis = """
                    SITUAÇÃO ATUAL DA LOJA
                    Dinheiro disponível: R$30.000
                    Receita média mensal: R$25.000
                    Despesas médias mensais: R$20.000
                    Resultado médio mensal: R$5.000
                    Reserva atual para emergências: R$5.000

                    OPÇÃO A — INVESTIR EM PUBLICIDADE
                    Custo: R$12.000
                    Aumento estimado nas vendas: 15%
                    Prazo para observar resultados: 3 meses
                    Risco: o aumento nas vendas não é garantido

                    OPÇÃO B — COMPRAR EQUIPAMENTOS
                    Custo: R$18.000
                    Economia estimada nas despesas: R$1.500 por mês
                    Prazo de instalação: 1 mês
                    Risco: manutenção e custos adicionais

                    OPÇÃO C — AUMENTAR A RESERVA
                    Valor sugerido para guardar: R$20.000
                    Benefício: maior segurança em períodos de dificuldade
                    Resultado financeiro imediato: nenhum
                    Risco: deixar de aproveitar oportunidades de crescimento

                    INFORMAÇÕES IMPORTANTES
                    A loja precisa pagar R$20.000 por mês para manter suas atividades.
                    O aumento estimado de 15% nas vendas não significa um aumento de 15% no lucro.
                    Os investimentos podem ser combinados, desde que respeitem o dinheiro disponível.
                    """,

                Objetivo =
                    "Criar uma estratégia para utilizar os recursos da empresa, considerando oportunidades de crescimento, economia de custos e segurança financeira.",

                PerguntaPrincipal =
                    "Como você distribuiria os R$30.000 disponíveis? Quais opções priorizaria, quais riscos consideraria e como avaliaria se sua decisão trouxe bons resultados?",

                Dificuldade = NivelDificuldade.Dificil,
                Categoria = categoriaFinancas
            });
        }

        // =====================================================
        // 3. SALVAR NO POSTGRESQL
        // =====================================================

        await context.SaveChangesAsync();
    }
}
