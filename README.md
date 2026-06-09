# 90 Seconds Stealing Simulator

## Sobre o Jogo
O jogador assume o papel de um ladrao que precisa de invadir uma casa gerada proceduralmente, recolher os itens de maior valor sem ultrapassar o limite de peso da mochila e escapar antes que o tempo acabe. Tudo isto enquanto se desvia do cone de visao de guardas que patrulham o local.

### Gêneros do jogo: Stealth e Puzzle

## Algoritmos Implementados

- Particionamento Binario de Espaco (BSP): Utilizado para a geracao procedural do mapa. O algoritmo divide a area principal recursivamente para criar salas de tamanhos variados, ligando-as por corredores, garantindo um mapa unico e funcional a cada partida.

- Problema da Mochila 0/1 (Knapsack - Programacao Dinamica): Utilizado no sistema de pontuacao e eficiencia. No final da fase, a IA constroi uma matriz de possibilidades avaliando o peso e valor de todos os itens instanciados na casa. Calcula o "roubo perfeito possivel" e compara-o com o que o jogador levou, gerando a percentagem de sucesso final.

- Problema do Caixeiro Viajante (TSP - Heuristica do Vizinho Mais Proximo): Controla o planeamento macro da Inteligencia Artificial. Le a posicao de todas as salas geradas e traca a ordem de visitacao mais curta para que o guarda patrulhe a casa toda e volte a origem.

- Busca em Largura (BFS - Pathfinding em Grid): Controla a navegacao micro da Inteligencia Artificial. Calcula as rotas celula a celula no Tilemap, permitindo que o inimigo contorne paredes dinamicas e persiga o jogador sem ficar preso no cenario.

## Como Rodar o Projeto

- Baixe a release mais recente na pagina do GitHub
- Extraia o arquivo .zip no seu computador
- Entre na pasta 90SSS extraida e abra o arquivo 90SSS.exe
