# enviar.ps1: empacota o projeto, envia para o VPS e reconstrói o sistema

$servidor = "root@187.41.119.192"
$pastaProjeto = Join-Path $PSScriptRoot "sistemasconsultas"
$pacote = Join-Path $env:USERPROFILE "projeto.tar.gz"

# Arquivo vazio significa que faltou salvar no Visual Studio
foreach ($nome in @("Dockerfile", "docker-compose.prod.yml", ".dockerignore", "Program.cs")) {
    $arquivo = Join-Path $pastaProjeto $nome
    if (-not (Test-Path $arquivo)) { throw "Arquivo nao encontrado: $nome" }
    if ((Get-Item $arquivo).Length -lt 20) { throw "Arquivo vazio (salve com Ctrl+S): $nome" }
}

Write-Host "1/3 Empacotando..."
tar -czf $pacote --exclude=bin --exclude=obj --exclude=.vs --exclude=.env -C $pastaProjeto .
if ($LASTEXITCODE -ne 0) { throw "Falha ao empacotar." }

Write-Host "2/3 Enviando para o servidor (vai pedir a senha)..."
scp $pacote "${servidor}:/root/"
if ($LASTEXITCODE -ne 0) { throw "Falha ao enviar." }

Write-Host "3/3 Atualizando o sistema no servidor (vai pedir a senha)..."
$comando = "set -e; rm -rf /opt/consultas-novo; mkdir -p /opt/consultas-novo; tar -xzf /root/projeto.tar.gz -C /opt/consultas-novo; cp /opt/consultas/.env /opt/consultas-novo/.env; rm -rf /opt/consultas; mv /opt/consultas-novo /opt/consultas; cd /opt/consultas; docker compose -f docker-compose.prod.yml up -d --build"
ssh $servidor $comando
if ($LASTEXITCODE -ne 0) { throw "Falha ao atualizar no servidor." }

Write-Host "Pronto! Abra http://187.41.119.192"