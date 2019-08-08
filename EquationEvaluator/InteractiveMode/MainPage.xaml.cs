using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Core;
using System.Threading.Tasks;

namespace InteractiveMode
{
    public sealed partial class MainPage : Page
    {
        //Used to detect and analyze writing
        InkAnalyzer analyzerText = new InkAnalyzer();
        DispatcherTimer timer = new DispatcherTimer();

        //Used to create and manage math equations
        private int answer;
        private int[] numbers;
        private char mathOperator;

        //Initializes components and generates a new equation
        public MainPage()
        {
            this.InitializeComponent();

            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Touch |
                Windows.UI.Core.CoreInputDeviceTypes.Pen;

            inkCanvas.InkPresenter.StrokesCollected += inkCanvas_StrokesCollected;
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += inkCanvas_StrokeStarted;

            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_TickAsync;

            numbers = new int[2];

            generateNewEquation();
        }

        //Creates a new equation with a random operator and random numbers
        private void generateNewEquation()
        {
            Random random = new Random();
            int temp = random.Next(3);
            if (temp == 0)
                mathOperator = '+';
            else if (temp == 1)
                mathOperator = '-';
            else
                mathOperator = '*';

            textBlock1.Text = mathOperator + "";

            if (mathOperator == '+')
            {
                numbers[0] = random.Next(51);
                numbers[1] = random.Next(51);

                answer = numbers[0] + numbers[1];
            }
            else if (mathOperator == '-')
            {
                numbers[0] = random.Next(101);
                numbers[1] = random.Next(numbers[0] + 1);

                answer = numbers[0] - numbers[1];
            }
            else if (mathOperator == '*')
            {
                numbers[0] = random.Next(11);
                numbers[1] = random.Next(11);

                answer = numbers[0] * numbers[1];
            }

            textBlock0.Text = numbers[0] + "";
            textBlock2.Text = numbers[1] + "";
            textBlock3.Text = "=";

            Info.Text = "";
        }

        //Recognizes text from the ink strokes
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
                        int userAnswer = 0;

                        if (Int32.TryParse(word.RecognizedText, out userAnswer))
                        {

                            if (userAnswer == answer)
                            {
                                Info.Text = "Correct!";

                                await Task.Delay(TimeSpan.FromSeconds(2));

                                generateNewEquation();
                            }
                            else
                            {
                                Info.Text = "Wrong, try again!";

                                await Task.Delay(TimeSpan.FromSeconds(2));

                                Info.Text = "";
                            }
                        }

                        foreach (var strokeId in word.GetStrokeIds())
                        {
                            var stroke = inkCanvas.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                            stroke.Selected = true;
                        }
                        analyzerText.RemoveDataForStrokes(word.GetStrokeIds());
                    }
                    inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
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

            foreach (var stroke in args.Strokes)
            {
                analyzerText.AddDataForStroke(stroke);
                analyzerText.SetStrokeDataKind(stroke.Id, InkAnalysisStrokeKind.Writing);
            }

            timer.Start();
        }
    }
}
