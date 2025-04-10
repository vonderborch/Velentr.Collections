using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Velentr.INDIVIDUAL_SUPPORT.TestApp;

/// <summary>
/// A class that represents a frame metric.
/// </summary>
internal class FrameMetric
{
    /// <summary>
    /// The samples.
    /// </summary>
    private readonly Dictionary<int, double> samples;

    /// <summary>
    /// The current index.
    /// </summary>
    private int currentIndex;

    /// <summary>
    /// The maximum number of samples to keep.
    /// </summary>
    private readonly int maximumSamples;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameMetric"/> class.
    /// </summary>
    /// <param name="maximumSamples">The maximum number of samples to keep.</param>
    internal FrameMetric(int maximumSamples)
    {
        this.maximumSamples = maximumSamples;
        this.samples = new Dictionary<int, double>(maximumSamples);
    }

    /// <summary>
    /// The average frames per second.
    /// </summary>
    public double AverageFramesPerSecond { get; private set; }

    /// <summary>
    /// Records a frame.
    /// </summary>
    /// <param name="timeSpan">The timespan since the last instance.</param>
    public void AddFrame(TimeSpan timeSpan)
    {
        this.samples[this.currentIndex] = 1d / timeSpan.TotalSeconds;
        this.AverageFramesPerSecond = this.samples.Average(pair => pair.Value);
        this.currentIndex = (this.currentIndex + 1) % this.maximumSamples;
    }
}

/// <summary>
/// A class that represents CPU and Memory metrics.
/// </summary>
internal class ResourceMetrics : IDisposable
{
    /// <summary>
    /// The process.
    /// </summary>
    private Process process;

    /// <summary>
    /// The garbage collection memory samples.
    /// </summary>
    private readonly Dictionary<int, long> gcMemorySamples;

    /// <summary>
    /// The memory samples.
    /// </summary>
    private readonly Dictionary<int, long> memorySamples;

    /// <summary>
    /// The CPU samples.
    /// </summary>
    private readonly Dictionary<int, double> cpuSamples;

    /// <summary>
    /// The current index.
    /// </summary>
    private int currentIndex;

    /// <summary>
    /// The previous time.
    /// </summary>
    private DateTime previousTime;

    /// <summary>
    /// The previous processor time.
    /// </summary>
    private TimeSpan previousProcessorTime;

    /// <summary>
    /// The sleep time in milliseconds.
    /// </summary>
    private readonly int sleepTimeMilliseconds;

    /// <summary>
    /// The cancellation token.
    /// </summary>
    private CancellationTokenSource cancellationToken;

    /// <summary>
    /// The update thread.
    /// </summary>
    private Task updateThread;

    /// <summary>
    /// The maximum number of samples to keep.
    /// </summary>
    private readonly int maximumSamples;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceMetrics"/> class.
    /// </summary>
    /// <param name="maximumSamples">The maximum samples to keep.</param>
    /// <param name="sleepTimeMilliseconds">The update thead sleep time in milliseconds.</param>
    internal ResourceMetrics(int maximumSamples, int sleepTimeMilliseconds = 500)
    {
        this.process = Process.GetCurrentProcess();
        this.previousProcessorTime = this.process.TotalProcessorTime;
        this.previousTime = DateTime.Now;

        this.maximumSamples = maximumSamples;
        this.memorySamples = new Dictionary<int, long>(maximumSamples);
        this.gcMemorySamples = new Dictionary<int, long>(maximumSamples);
        this.cpuSamples = new Dictionary<int, double>(maximumSamples);
        this.cancellationToken = new CancellationTokenSource();
        this.sleepTimeMilliseconds = sleepTimeMilliseconds;
        this.updateThread = new Task(UpdateTask, this.cancellationToken.Token);
        this.updateThread.Start();
    }

    /// <summary>
    /// The average memory usage in MB.
    /// </summary>
    public double AverageMemoryUsageMb { get; private set; }

    /// <summary>
    /// The average garbage collection memory usage in MB.
    /// </summary>
    public double AverageGcMemoryUsageMb { get; private set; }

    /// <summary>
    /// The average CPU percent.
    /// </summary>
    public double AverageCpuPercent { get; private set; }

    /// <summary>
    /// The update method.
    /// </summary>
    public void Update()
    {
        if (this.updateThread.IsCompleted)
        {
            this.updateThread = new Task(UpdateTask, this.cancellationToken.Token);
            this.updateThread.Start();
        }
        else if (this.updateThread.IsFaulted)
        {
            this.cancellationToken = new CancellationTokenSource();
            this.updateThread = new Task(UpdateTask, this.cancellationToken.Token);
            this.updateThread.Start();
        }
    }

    /// <summary>
    /// The update task.
    /// </summary>
    private void UpdateTask()
    {
        while (!this.cancellationToken.Token.IsCancellationRequested)
        {
            this.process.Refresh();
            var newTime = DateTime.Now;
            var newProcessorTime = this.process.TotalProcessorTime;

            this.gcMemorySamples[this.currentIndex] = GC.GetTotalMemory(false);
            this.memorySamples[this.currentIndex] = this.process.WorkingSet64;

            var cpuUsedMilliseconds = (newProcessorTime - this.previousProcessorTime).TotalMilliseconds;
            var totalMilliseconds = (newTime - this.previousTime).TotalMilliseconds;
            this.cpuSamples[this.currentIndex] =
                cpuUsedMilliseconds / (Environment.ProcessorCount * totalMilliseconds) * 100d;

            this.AverageGcMemoryUsageMb = this.gcMemorySamples.Average(pair => pair.Value) / 1024d / 1024d;
            this.AverageMemoryUsageMb = this.memorySamples.Average(pair => pair.Value) / 1024d / 1024d;
            this.AverageCpuPercent = this.cpuSamples.Average(pair => pair.Value);

            this.currentIndex = (this.currentIndex + 1) % this.maximumSamples;
            this.previousTime = newTime;
            this.previousProcessorTime = newProcessorTime;
            Thread.Sleep(this.sleepTimeMilliseconds);
        }
    }

    /// <summary>
    /// Disposes of the object.
    /// </summary>
    public void Dispose()
    {
        this.cancellationToken.Cancel();
        this.updateThread.Wait();
        this.cancellationToken.Dispose();
    }
}

/// <summary>
/// A class that represents a performance monitored game.
/// </summary>
public class PerformanceMonitoredGame : Game
{
    /// <summary>
    /// The decimals to round to.
    /// </summary>
    private const string DECIMALS = "0.00";

    /// <summary>
    /// The metrics text formats.
    /// </summary>
    private readonly string[] metricsFormats;

    /// <summary>
    /// The frame counter.
    /// </summary>
    private readonly FrameMetric frameCounter;

    /// <summary>
    /// The tick counter.
    /// </summary>
    private readonly FrameMetric tickCounter;

    /// <summary>
    /// The resource metrics.
    /// </summary>
    private readonly ResourceMetrics resourceMetrics;

    /// <summary>
    /// The font.
    /// </summary>
    private SpriteFont font;

    /// <summary>
    /// The font name.
    /// </summary>
    private string fontName;

    /// <summary>
    /// The position to render the metrics at.
    /// </summary>
    private Vector2 metricsPosition;

    /// <summary>
    /// The font color.
    /// </summary>
    private Color fontColor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitoredGame"/> class.
    /// </summary>
    /// <param name="title">The title of the game.</param>
    /// <param name="version">The version of the game.</param>
    /// <param name="framework">The framework the game is running on.</param>
    /// <param name="font">The font to use for metrics.</param>
    /// <param name="fontColor">The color of the font to use for metrics.</param>
    /// <param name="metricsPosition">The position to render the metrics at.</param>
    /// <param name="maximumSamples">The number of samples to keep.</param>
    public PerformanceMonitoredGame(string title, string version, string framework, string font, Color fontColor,
        Vector2 metricsPosition, int maximumSamples = 100) : base()
    {
        this.fontName = font;
        this.fontColor = fontColor;
        this.metricsPosition = metricsPosition;

        this.Window.Title = $"{title} ({framework}) | {version}";
        this.metricsFormats =
        [
            $"FPS: {{0:{DECIMALS}}} | TPS: {{1:{DECIMALS}}}",
            $"CPU: {{2:{DECIMALS}}}%",
            $"GC/WS Mem: {{3:{DECIMALS}}} MB / {{4:{DECIMALS}}} MB"
        ];

        this.frameCounter = new FrameMetric(maximumSamples);
        this.tickCounter = new FrameMetric(maximumSamples);
        this.resourceMetrics = new ResourceMetrics(maximumSamples);
    }

    /// <summary>
    /// Loads the content.
    /// </summary>
    protected override void LoadContent()
    {
        this.font = this.Content.Load<SpriteFont>(this.fontName);
    }

    /// <summary>
    ///     Updates the given gameTime.
    /// </summary>
    /// <param name="gameTime"> The game time. </param>
    protected override void Update(GameTime gameTime)
    {
        this.tickCounter.AddFrame(gameTime.ElapsedGameTime);
        this.resourceMetrics.Update();
        base.Update(gameTime);
    }

    /// <summary>
    ///     Draws the given game time.
    /// </summary>
    /// <param name="gameTime"> The game time. </param>
    /// <param name="spriteBatch"> The sprite batch. </param>
    protected void RenderMetrics(GameTime gameTime, SpriteBatch spriteBatch)
    {
        this.frameCounter.AddFrame(gameTime.ElapsedGameTime);

        var startPosition = this.metricsPosition;
        foreach (var metricFormat in this.metricsFormats)
        {
            var text = string.Format(
                metricFormat,
                this.frameCounter.AverageFramesPerSecond,
                this.tickCounter.AverageFramesPerSecond,
                this.resourceMetrics.AverageCpuPercent,
                this.resourceMetrics.AverageGcMemoryUsageMb,
                this.resourceMetrics.AverageMemoryUsageMb
            );

            spriteBatch.DrawString(this.font, ToSafeText(text), startPosition, this.fontColor);
            startPosition.Y += this.font.LineSpacing;
        }
    }

    /// <summary>
    /// Ensures that the text is safe to render.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The safe text.</returns>
    private string ToSafeText(string text)
    {
        StringBuilder finalString = new();

        foreach (var c in text)
        {
            if (this.font.Characters.Contains(c))
            {
                finalString.Append(c);
            }
            else
            {
                finalString.Append('?');
            }
        }

        return finalString.ToString();
    }

    /// <summary>
    /// Disposes of the object.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        this.resourceMetrics.Dispose();

        base.Dispose(disposing);
    }
}
