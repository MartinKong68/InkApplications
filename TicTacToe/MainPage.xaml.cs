using System;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Core;
using Windows.UI;

namespace TicTacToe
{
    public sealed partial class MainPage : Page
    {
        private InkCanvas[] inkCanvas;
        private Canvas[] canvas;
        InkAnalyzer analyzerText = new InkAnalyzer();
        DispatcherTimer timer = new DispatcherTimer();
        int currentCanvasNumber = 100;

        private char[] gridResults;
        private bool isXNext;
        private bool gameEnded;
        public MainPage()
        {
            this.InitializeComponent();

            inkCanvas = new InkCanvas[9];
            inkCanvas[0] = inkCanvas0;
            inkCanvas[1] = inkCanvas1;
            inkCanvas[2] = inkCanvas2;
            inkCanvas[3] = inkCanvas3;
            inkCanvas[4] = inkCanvas4;
            inkCanvas[5] = inkCanvas5;
            inkCanvas[6] = inkCanvas6;
            inkCanvas[7] = inkCanvas7;
            inkCanvas[8] = inkCanvas8;

            for (int i = 0; i < inkCanvas.Length; i++)
            {
                inkCanvas[i].InkPresenter.InputDeviceTypes =
                    Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                    Windows.UI.Core.CoreInputDeviceTypes.Touch |
                    Windows.UI.Core.CoreInputDeviceTypes.Pen;

                inkCanvas[i].InkPresenter.StrokesCollected += inkCanvas_StrokesCollected;
                inkCanvas[i].InkPresenter.StrokeInput.StrokeStarted += inkCanvas_StrokeStarted;
            }

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_TickAsync;

            canvas = new Canvas[9];
            canvas[0] = canvas0;
            canvas[1] = canvas1;
            canvas[2] = canvas2;
            canvas[3] = canvas3;
            canvas[4] = canvas4;
            canvas[5] = canvas5;
            canvas[6] = canvas6;
            canvas[7] = canvas7;
            canvas[8] = canvas8;

            gridResults = new char[9];

            NewGame();
        }

        private void NewGame()
        {
            for (int i = 0; i < canvas.Length; i++)
            {
                canvas[i].Children.Clear();
                canvas[i].Background = new SolidColorBrush();
            }

            for (int i = 0; i < gridResults.Length; i++)
                gridResults[i] = ' ';

            gameEnded = false;

            isXNext = true;

            Info.Text = "Draw an 'X' in any square to begin!";
        }

        private void checkForWinner()
        {
            if (gridResults[0] != ' ' && (gridResults[0] & gridResults[1] & gridResults[2]) == gridResults[0])
            {
                gameEnded = true;

                canvas[0].Background = new SolidColorBrush(Colors.Aqua);
                canvas[1].Background = new SolidColorBrush(Colors.Aqua);
                canvas[2].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[3] != ' ' && (gridResults[3] & gridResults[4] & gridResults[5]) == gridResults[3])
            {
                gameEnded = true;

                canvas[3].Background = new SolidColorBrush(Colors.Aqua);
                canvas[4].Background = new SolidColorBrush(Colors.Aqua);
                canvas[5].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[6] != ' ' && (gridResults[6] & gridResults[7] & gridResults[8]) == gridResults[6])
            {
                gameEnded = true;

                canvas[6].Background = new SolidColorBrush(Colors.Aqua);
                canvas[7].Background = new SolidColorBrush(Colors.Aqua);
                canvas[8].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[0] != ' ' && (gridResults[0] & gridResults[3] & gridResults[6]) == gridResults[0])
            {
                gameEnded = true;

                canvas[0].Background = new SolidColorBrush(Colors.Aqua);
                canvas[3].Background = new SolidColorBrush(Colors.Aqua);
                canvas[6].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[1] != ' ' && (gridResults[1] & gridResults[4] & gridResults[7]) == gridResults[1])
            {
                gameEnded = true;

                canvas[1].Background = new SolidColorBrush(Colors.Aqua);
                canvas[4].Background = new SolidColorBrush(Colors.Aqua);
                canvas[7].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[2] != ' ' && (gridResults[2] & gridResults[5] & gridResults[8]) == gridResults[2])
            {
                gameEnded = true;

                canvas[2].Background = new SolidColorBrush(Colors.Aqua);
                canvas[5].Background = new SolidColorBrush(Colors.Aqua);
                canvas[8].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[0] != ' ' && (gridResults[0] & gridResults[4] & gridResults[8]) == gridResults[0])
            {
                gameEnded = true;

                canvas[0].Background = new SolidColorBrush(Colors.Aqua);
                canvas[4].Background = new SolidColorBrush(Colors.Aqua);
                canvas[8].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (gridResults[2] != ' ' && (gridResults[2] & gridResults[4] & gridResults[6]) == gridResults[2])
            {
                gameEnded = true;

                canvas[2].Background = new SolidColorBrush(Colors.Aqua);
                canvas[4].Background = new SolidColorBrush(Colors.Aqua);
                canvas[6].Background = new SolidColorBrush(Colors.Aqua);
            }

            if (!gridResults.Any(f => f == ' '))
            {
                gameEnded = true;

                canvas[0].Background = new SolidColorBrush(Colors.Orange);
                canvas[1].Background = new SolidColorBrush(Colors.Orange);
                canvas[2].Background = new SolidColorBrush(Colors.Orange);
                canvas[3].Background = new SolidColorBrush(Colors.Orange);
                canvas[4].Background = new SolidColorBrush(Colors.Orange);
                canvas[5].Background = new SolidColorBrush(Colors.Orange);
                canvas[6].Background = new SolidColorBrush(Colors.Orange);
                canvas[7].Background = new SolidColorBrush(Colors.Orange);
                canvas[8].Background = new SolidColorBrush(Colors.Orange);
            }

            if (gameEnded)
            {
                Info.Text = "Game Over! To start a new game, draw anywhere.";
            }
        }
        private async void timer_TickAsync(object sender, object e)
        {
            timer.Stop();

            if (!analyzerText.IsAnalyzing)
            {
                InkAnalysisResult resultText = await analyzerText.AnalyzeAsync();

                if (resultText.Status == InkAnalysisStatus.Updated)
                {
                    var words = analyzerText.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkWord);

                    foreach (InkAnalysisInkWord word in words)
                    {
                        if (gameEnded)
                        {
                            NewGame();
                        }
                        else if (gridResults[currentCanvasNumber] == ' ')
                        {
                            if (isXNext && (word.RecognizedText == "x" || word.RecognizedText == "X"))
                            {
                                DrawText(word.RecognizedText, word.BoundingRect);

                                gridResults[currentCanvasNumber] = 'x';

                                isXNext = !isXNext;

                                Info.Text = "Now it is Player 2's turn. Draw an 'O' in any remaining square.";
                            }
                            else if (!isXNext && (word.RecognizedText == "o" || word.RecognizedText == "O" || word.RecognizedText == "0"))
                            {
                                DrawText(word.RecognizedText, word.BoundingRect);

                                gridResults[currentCanvasNumber] = 'o';

                                isXNext = !isXNext;

                                Info.Text = "Now it is Player 1's turn. Draw an 'X' in any remaining square.";
                            }
                        }

                        foreach (var strokeId in word.GetStrokeIds())
                        {
                            var stroke = inkCanvas[currentCanvasNumber].InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                            stroke.Selected = true;
                        }
                        analyzerText.RemoveDataForStrokes(word.GetStrokeIds());
                    }
                    inkCanvas[currentCanvasNumber].InkPresenter.StrokeContainer.DeleteSelected();
                }
                else
                {
                    timer.Start();
                }

                analyzerText.ClearDataForAllStrokes();

                checkForWinner();
            }
        }
        private void inkCanvas_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            timer.Stop();
        }

        private void inkCanvas_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            timer.Stop();

            for (int i = 0; i < inkCanvas.Length; i++)
            {
                if (inkCanvas[i].InkPresenter == sender)
                    currentCanvasNumber = i;
            }

            foreach (var stroke in args.Strokes)
            {
                analyzerText.AddDataForStroke(stroke);
                analyzerText.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
            }

            timer.Start();
        }
        private void DrawText(string recognizedText, Rect boundingRect)
        {
            TextBlock text = new TextBlock();
            Canvas.SetTop(text, boundingRect.Top);
            Canvas.SetLeft(text, boundingRect.Left);

            text.Text = recognizedText;
            text.FontSize = boundingRect.Height;

            canvas[currentCanvasNumber].Children.Add(text);
        }
    }
}
