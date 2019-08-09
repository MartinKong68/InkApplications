using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Core;

namespace ExpressionEvaluator
{
    public sealed partial class MainPage : Page
    {
        //Store the individual ink canvases and text blocks
        private InkCanvas[] inkCanvas;
        private TextBlock[] textBlock;

        //Used to detect and analyze writing
        InkAnalyzer analyzerText = new InkAnalyzer();
        DispatcherTimer timer = new DispatcherTimer();

        //Used to manage and solve math equations 
        private int currentCanvasNumber = 100;
        private bool[] hasValues;
        private int[] numbers;
        private char mathOperator;
        private bool calculated;

        //Initializes the different components of the application
        public MainPage()
        {
            this.InitializeComponent();

            inkCanvas = new InkCanvas[4];
            inkCanvas[0] = inkCanvas0;
            inkCanvas[1] = inkCanvas1;
            inkCanvas[2] = inkCanvas2;
            inkCanvas[3] = inkCanvas3;

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

            textBlock = new TextBlock[5];
            textBlock[0] = textBlock0;
            textBlock[1] = textBlock1;
            textBlock[2] = textBlock2;
            textBlock[3] = textBlock3;
            textBlock[4] = textBlock4;

            hasValues = new bool[2];

            clear();
        }

        //Clears the text blocks and sets everything to their default state
        private void clear()
        {
            for (int i = 0; i < textBlock.Length; i++)
                textBlock[i].Text = "";

            numbers = new int[2];

            hasValues[0] = false;
            hasValues[1] = false;

            mathOperator = ' ';

            Info.Text = "Enter any equation you want to be solved in the designated boxes.";

            calculated = false;
        }

        //Calculates the math equation given the two numbers and the operator
        private void calculate()
        {
            int answer = 0;

            if (mathOperator == '/')
            {
                answer = numbers[0] / numbers[1];
            }
            else if (mathOperator == '*')
            {
                answer = numbers[0] * numbers[1];
            }
            else if (mathOperator == '-')
            {
                answer = numbers[0] - numbers[1];
            }
            else if (mathOperator == '+')
            {
                answer = numbers[0] + numbers[1];
            }

            textBlock[4].Text = answer + "";

            Info.Text = "Draw in any of the boxes except for the right one in order to clear everything.";

            calculated = true;
        }

        //Checks if the user entered the parameters for the equation
        private bool canCalculate()
        {
            return hasValues[0] && hasValues[1] && mathOperator != ' ';
        }

        //Recognizes text from ink strokes
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
                        if (calculated)
                        {
                            clear();
                        }
                        else if (currentCanvasNumber == 0)
                        {
                            int temp = numbers[0];

                            if (Int32.TryParse(word.RecognizedText, out numbers[0]))
                            {
                                textBlock[0].Text = numbers[0] + "";

                                hasValues[0] = true;

                            }
                            else if (hasValues[0])
                                numbers[0] = temp;
                        }
                        else if (currentCanvasNumber == 1)
                        {
                            if (word.RecognizedText == "/")
                            {
                                mathOperator = '/';

                                textBlock[1].Text = "/";
                            }
                            else if (word.RecognizedText == "*" || word.RecognizedText == "x" || word.RecognizedText == "X")
                            {
                                mathOperator = '*';

                                textBlock[1].Text = "*";
                            }
                            else if (word.RecognizedText == "+")
                            {
                                mathOperator = '+';

                                textBlock[1].Text = "+";
                            }
                            else if (word.RecognizedText == "-" || word.RecognizedText == '_')
                            {
                                mathOperator = '-';

                                textBlock[1].Text = "-";
                            }
                        }
                        else if (currentCanvasNumber == 2)
                        {
                            int temp = numbers[1];

                            if (Int32.TryParse(word.RecognizedText, out numbers[1]))
                            {
                                textBlock[2].Text = numbers[1] + "";

                                hasValues[1] = true;
                            }
                            else if (hasValues[1])
                                numbers[1] = temp;
                        }
                        else if (currentCanvasNumber == 3)
                        {
                            if (word.RecognizedText == "=" && canCalculate())
                            {
                                textBlock[3].Text = "=";

                                calculate();
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
            }
        }

        //Detects when writing starts
        private void inkCanvas_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            timer.Stop();
        }

        //Detects when the writing strokes are finished
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
    }
}
