# Relatório de testes — 27/11/2026 00:35

- **Modo:** com janela, monitor 2 (1280×720)  
- **Sementes:** 20261126, 777  
- **Duração:** 00:06:11  
- **Resultado:** 1 EXECUÇÃO(ÕES) COM FALHA (493/494 verificações)

| Grupo | Execução | Resultado | Tempo |
|---|---|---|---|
| Unidade | Testes de unidade | OK 33/33 | 1 s |
| Prólogo (Vale) | prologue | OK 32/32 | 35 s |
| Prólogo (Vale) | resume-vale | OK 5/5 | 2 s |
| Prólogo (Vale) | resume-ending | OK 2/2 | 2 s |
| Prólogo (Vale) | old-save | OK 2/2 | 2 s |
| Prólogo (Vale) | skip | OK 2/2 | 4 s |
| Recuperação do protótipo | vale | OK 20/20 | 9 s |
| Recuperação do protótipo | vale2 | OK 3/3 | 7 s |
| Recuperação do protótipo | controls | OK 13/13 | 4 s |
| Recuperação do protótipo | controls2 | OK 4/4 | 3 s |
| Recuperação do protótipo | assist | OK 5/5 | 2 s |
| Recuperação do protótipo | assist2 | OK 2/2 | 2 s |
| Recuperação do protótipo | book | OK 7/7 | 3 s |
| Recuperação do protótipo | layout | OK 7/7 | 7 s |
| Novidades (pedido 13) | aim | OK 12/12 | 9 s |
| Novidades (pedido 13) | charge | **FALHA** 17/18 | 21 s |
| Novidades (pedido 13) | charge-plains | OK 4/4 | 7 s |
| Novidades (pedido 13) | reinforce | OK 17/17 | 17 s |
| Novidades (pedido 13) | bag | OK 11/11 | 5 s |
| Novidades (pedido 13) | money | OK 8/8 | 3 s |
| Novidades (pedido 13) | money2 | OK 1/1 | 2 s |
| Novidades (pedido 13) | oldsave | OK 4/4 | 3 s |
| Técnicas seladas | full | OK 3/3 | 7 s |
| Técnicas seladas | sealed | OK 19/19 | 16 s |
| Técnicas seladas | resume2 | OK 2/2 | 2 s |
| Técnicas seladas | resume3 | OK 1/1 | 2 s |
| Técnicas seladas | migrate | OK 2/2 | 2 s |
| Técnicas seladas | migrate2 | OK 1/1 | 2 s |
| Tela e mapa | measure | OK 5/5 | 8 s |
| Tela e mapa | click | OK 8/8 | 5 s |
| Tela e mapa | map | OK 9/9 | 3 s |
| Tela e mapa | map2 | OK 2/2 | 2 s |
| Controles | Teclado e mouse | OK 14/14 | 11 s |
| Planície | semente 20261126 | OK 44/44 | 51 s |
| Jornada | nova, semente 20261126 | OK 31/31 | 21 s |
| Jornada | retomada, semente 20261126 | OK 11/11 | 2 s |
| Ambientação | dia, semente 20261126 | OK 15/15 | 8 s |
| Ambientação | noite, semente 20261126 | OK 6/6 | 2 s |
| Ambientação | retomada da hora, semente 20261126 | OK 1/1 | 2 s |
| Planície | semente 777 | OK 44/44 | 46 s |
| Jornada | nova, semente 777 | OK 31/31 | 20 s |
| Jornada | retomada, semente 777 | OK 11/11 | 2 s |
| Ambientação | dia, semente 777 | OK 15/15 | 8 s |
| Ambientação | noite, semente 777 | OK 6/6 | 2 s |
| Ambientação | retomada da hora, semente 777 | OK 1/1 | 2 s |

## Falhas

### Novidades (pedido 13) · charge
- a partial charge against the Closed Guard is parried (no break) — vida 12→12
- log: `Novidades_pedido_13_-charge.log`

