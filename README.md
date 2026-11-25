# 🚗 Crypto Run

**Jogo de corrida em 3D desenvolvido na Unity 6 por Orion Estudio.**

Dirija pela cidade, corra contra o relógio e colete os 4 contratos.
Quanto mais rápido você chegar, mais **BTC** você ganha.

---

## 📖 Sobre o jogo

Crypto Run é um jogo de direção com foco em tempo e precisão.
Uma seta guia o jogador até cada contrato, e o pagamento depende de quanto tempo
sobrou ao chegar. A cidade é viva: carros, pedestres e cachorros
circulam pelas ruas e reagem às batidas.

## 🎮 Como jogar

1. Siga a **seta** até o primeiro contrato e passe por ele para coletar.
2. Ao coletar, o tempo **reinicia** e um novo prazo começa para o próximo contrato.
3. Colete os **4 contratos** para vencer.

O valor de cada contrato depende da cor da barra de tempo no momento da coleta:

| Barra de tempo | Pagamento |
|---|---|
| 🟢 Verde | Valor máximo |
| 🟡 Amarelo | Valor médio |
| 🔴 Vermelho | Valor mínimo |

Se o tempo zerar, é **Game Over**. Aperte o botão de Pause para recomeçar.

## ⌨️ Controles (padrão)

Todos os botões podem ser trocados no menu **Controles**, tanto no teclado quanto no joystick.

| Ação | Teclado | Joystick |
|---|---|---|
| Acelerar (SPEED) | M | Botão 3 |
| Freio / Ré (BRAKE) | N | Botão 4 |
| Virar | A / D | Analógico (esquerda / direita) |
| Pause (START) | P | Botão 10 |
| Câmera interna (CAM INT) | Num 0 | Botão 6 |
| Câmera esquerda (CAM LEFT) | Num 7 | Botão 8 |
| Câmera direita (CAM RIGHT) | Num 8 | Botão 5 |
| Câmera frontal (CAM FRONT) | Num 9 | Botão 7 |

## ✨ Destaques técnicos

- **Remapeamento de controles** em tempo de jogo (teclado e joystick) com o novo Input System da Unity, e as escolhas ficam salvas.
- **Menu de pause** com navegação completa pelo controle.
- **Áudio com Audio Mixer**: volume de música e ambiência ajustável e salvo.
- **Sistema de danos**: a lataria amassa com Blend Shapes de acordo com a força do impacto, e o carro solta fumaça.
- **Cidade com movimento**: carros, pedestres e cachorros controlados por script, com física de ragdoll nos atropelamentos.
- **5 câmeras**: principal, interna, lateral esquerda, lateral direita e frontal.
- **Sons dinâmicos**: batidas, raspadas em parede, buzinas e motor.
- **Seta-guia 3D** e barra de tempo com troca de cor e alerta piscante na fase crítica.

## 🛠️ Tecnologias

- Unity 6 (6000.5.7f1), Universal Render Pipeline (URP)
- C#
- Unity Input System
- Audio Mixer, física com Rigidbody

## 📁 Estrutura do projeto

```
Assets/Meu game/
├── Scripts/          # Código do jogo (C#)
│   ├── Controles/    # Remapeamento e carregamento dos controles
│   ├── IniciarGame/  # Menu inicial e retorno ao menu
│   └── Som/          # Áudio, volumes e efeitos sonoros
├── InputActions/     # Mapa de controles (Input System)
├── Kits/ e CarKits/  # Prefabs montados para o jogo
├── Menu Inicial.unity
└── SampleScene.unity # Cena principal
```

> ⚠️ Os modelos 3D, texturas e efeitos sonoros de terceiros **não estão incluídos** neste repositório,
> por respeito às licenças da Unity Asset Store. Ao abrir o projeto, esses objetos aparecerão
> como ausentes. Para jogar, use a versão para Windows.

## 📥 Download

A demo jogável para Windows está disponível na página de Releases:

👉 **[Baixar Crypto Run v1.0 (Demo)](https://github.com/loazelan-blip/CryptoRun/releases/latest)**

1. Baixe o arquivo **CryptoRun_Windows.zip**.
2. Clique com o botão direito no arquivo e escolha **Extrair tudo**.
3. Abra a pasta extraída e dê dois cliques em **Crypto Run.exe**.

## 📦 Créditos

**Desenvolvimento, programação e game design:** Hadu – Orion Estudio

**Trilha sonora original:** Hadu – músicas autorais compostas para o jogo

**Recursos de terceiros** (Unity Asset Store):
Bublisher · Hodaart · Simple city plain · SimpleTownLite · Police Car & Helicopter ·
Low Poly Soviet Car Pack · Nebula · asset_free_Ukraine_cars · Car2 · FastMesh ·
Houses_Pack · ithappy · Pandazole · Studio Horizon · WhiteCity

Todos os direitos sobre esses recursos pertencem aos seus respectivos autores.

---

© 2026 Orion Estudio
