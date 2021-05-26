# unity_aircraft

![](https://github.com/karta134033/unity_aircraft/blob/master/game.gif?raw=true)
此篇研究基於Proximal Policy Optimization強化式學習演算法套用在飛行模擬遊戲中，目的在於研究在同一種應用、同一種演算法在不同的環境偵測方式下是否有顯著的差異。
研究應用在於自行開發的飛行模擬遊戲上，遊戲的內容、環境設置、參數調整皆為另外設計、開發。遊戲的內容為控制一台飛機，並試著穿越各個檢查點，只要穿越過檢查點後就會把時間往上加，當時間歸零或是飛機撞擊障礙物墜毀時將使遊戲結束，反之若能持續飛行並通過檢查點，且能在時間歸零以前通過所有檢查點則完成遊戲。

遊戲設置為控制飛機向上、向下、向左、向右飛行，並且能夠以按住空白鍵使飛機加速。其中，飛機加速的功能是為了最大化區別人為與演算法控制的差別，因在飛機加速的情況下能使速度提升十倍，屆時飛機將非常難以控制方向，使得一般玩家難以持續在加速的情況下遊玩。
論文的目標在於檢視哪種環境偵測方法將最大化遊戲表現。強化式學習演算法需要仰賴環境的輸入作為神經網路的訓練指標，因此需要一個偵測環境輸入的方式。在過往的論文中大多是使用卷積神經網路的方式將環境以圖片的方式做偵測，原因為大部分的論文研究目的是在演算法之間效能的差異，此外，測試環境常為市面上的遊戲，在這些遊戲中若要以卷積神經網路以外的方式感知環境將需要修改遊戲，效益上相對不高。

研究方法將分為三類，分別為:傳統的卷積神經網路方式、感測器感應環境的方式以及混和兩種方法的方式。其中感測器感應環境的方式確實比傳統的方式還要優秀，但更好的方式是結合兩種方法。研究中也發現傳統的方式會使訓練時間較大程度的拉長，且在高分辨率的圖片下做實驗對電腦的效能亦是個考驗。而結合兩種方法的方式在訓練時間上與傳統的方式差異不大，但在訓練與測試的表現都是最佳的。由實驗結果可得知，若在環境可控的情況下，傳統的偵測方式未必是最佳的。
