using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PensaAntes.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    AreaPreferencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Desafios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SituacaoProblema = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DadosDisponiveis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Objetivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PerguntaPrincipal = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Dificuldade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Desafios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Desafios_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Respostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    DesafioId = table.Column<int>(type: "integer", nullable: false),
                    AnaliseInicial = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    JustificativaInicial = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ConsultouIA = table.Column<bool>(type: "boolean", nullable: false),
                    AnaliseIA = table.Column<string>(type: "text", nullable: true),
                    DecisaoFinal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    JustificativaFinal = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFinalizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respostas", x => x.Id);
                    table.CheckConstraint("CK_Respostas_AnaliseIA_ConsultouIA", "\"AnaliseIA\" IS NULL OR \"ConsultouIA\" = TRUE");
                    table.ForeignKey(
                        name: "FK_Respostas_Desafios_DesafioId",
                        column: x => x.DesafioId,
                        principalTable: "Desafios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Respostas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AutoAvaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RespostaId = table.Column<int>(type: "integer", nullable: false),
                    OQueFezBem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OQuePodeMelhorar = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoAvaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoAvaliacoes_Respostas_RespostaId",
                        column: x => x.RespostaId,
                        principalTable: "Respostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RespostaId = table.Column<int>(type: "integer", nullable: false),
                    Identificacao = table.Column<int>(type: "integer", nullable: false),
                    UsoDosDados = table.Column<int>(type: "integer", nullable: false),
                    Hipotese = table.Column<int>(type: "integer", nullable: false),
                    Justificativa = table.Column<int>(type: "integer", nullable: false),
                    PensamentoCritico = table.Column<int>(type: "integer", nullable: false),
                    Feedback = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DataAvaliacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.CheckConstraint("CK_Avaliacoes_Hipotese", "\"Hipotese\" BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_Avaliacoes_Identificacao", "\"Identificacao\" BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_Avaliacoes_Justificativa", "\"Justificativa\" BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_Avaliacoes_PensamentoCritico", "\"PensamentoCritico\" BETWEEN 0 AND 3");
                    table.CheckConstraint("CK_Avaliacoes_UsoDosDados", "\"UsoDosDados\" BETWEEN 0 AND 3");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_Respostas_RespostaId",
                        column: x => x.RespostaId,
                        principalTable: "Respostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoAvaliacoes_RespostaId",
                table: "AutoAvaliacoes",
                column: "RespostaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_RespostaId",
                table: "Avaliacoes",
                column: "RespostaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nome",
                table: "Categorias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Desafios_CategoriaId",
                table: "Desafios",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_DesafioId",
                table: "Respostas",
                column: "DesafioId");

            migrationBuilder.CreateIndex(
                name: "IX_Respostas_UsuarioId",
                table: "Respostas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AutoAvaliacoes");

            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "Respostas");

            migrationBuilder.DropTable(
                name: "Desafios");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
