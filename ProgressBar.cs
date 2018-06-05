using System;

namespace ConsoleProgressBar
{
    public class ProgressBar
    {
        // Consts for basic default values
        private const string DEFAULT_LABEL = "";
        private const Options DEFAULT_OPTIONS = Options.Bookend | Options.DisplayPercentage;
        private const char DEFAULT_PROG_CHAR = '.';
        private const int DEFAULT_SIZE = 30;

        // Readonly fields set in constructors
        /// <summary>
        /// Optional custom label that appears to the left of the progress bar
        /// </summary>
        private readonly string _label;
        /// <summary>
        /// Flag enum indicating custom visual options
        /// </summary>
        private readonly Options _options;
        /// <summary>
        /// Character to use as a hash mark/increment on the progress bar
        /// </summary>
        private readonly char _progChar;
        /// <summary>
        /// Number of increments on the progress bar
        /// </summary>
        private readonly int _size;
        
        /// <summary>
        /// Running counter for num hash marks/increments drawn on the bar
        /// </summary>
        private int _charsDrawn = -1;
        /// <summary>
        /// Cursor position X coordinate for the percentage value appearing to the right of the progress bar
        /// </summary>
        private int? _percentStartPosX = null;
        /// <summary>
        /// Cursor position Y coordinate for the percentage value appearing to the right of the progress bar
        /// </summary>
        private int? _percentStartPosY = null;
        /// <summary>
        /// Cursor position X coordinate for the end of the progress bar
        /// </summary>
        private int _progBarEndPosX = -1;
        /// <summary>
        /// Cursor position Y coordinate for the end of the progress bar
        /// </summary>
        private int _progBarEndPosY = -1;
        /// <summary>
        /// Cursor position X coordinate for the start of the progress bar
        /// </summary>
        private int _progBarStartPosX = -1;
        /// <summary>
        /// Cursor position Y coordinate for the start of the progress bar
        /// </summary>
        private int _progBarStartPosY = -1;
        /// <summary>
        /// Running cursor position X coordinate of where the next hash mark/increment should be drawn on the progress bar
        /// </summary>
        private int _progBarToDrawPosX = -1;
        /// <summary>
        /// Running cursor position Y coordinate of where the next hash mark/increment should be drawn on the progress bar
        /// </summary>
        private int _progBarToDrawPosY = -1;


        /// <summary>
        /// Indicates whether or not the progress bar has completed
        /// </summary>
        public bool IsDone { get; private set; }
        /// <summary>
        /// Running counter of percent complete used to drive the progress bar
        /// </summary>
        public uint PercentComplete { get; private set; }


        /// <summary>
        /// Default constructor. Initializes values and writes initialized progress bar to the console.
        /// </summary>
        public ProgressBar()
        {
            _charsDrawn = 0;
            _label = DEFAULT_LABEL;
            _options = DEFAULT_OPTIONS;
            _progChar = DEFAULT_PROG_CHAR;
            _size = DEFAULT_SIZE;
            PercentComplete = 0;

            WriteSetupProgBar();
        }

        /// <summary>
        /// Parameterized constructor for a custom sized progress bar
        /// </summary>
        /// <param name="sizeIn">Number of increments in the progress bar</param>
        public ProgressBar(int sizeIn)
        {
            _charsDrawn = 0;
            _options = DEFAULT_OPTIONS;
            _progChar = DEFAULT_PROG_CHAR;
            _size = sizeIn;
            PercentComplete = 0;

            WriteSetupProgBar();
        }

        /// <summary>
        /// Parameterized constructor for a custom sized progress bar accommodating custom visual options
        /// </summary>
        /// <param name="sizeIn">Number of increments in the progress bar</param>
        /// <param name="optionsIn">Options to control visual appearance of the progress bar</param>
        /// <param name="labelIn">Custom label to appear to the left of the progress bar</param>
        public ProgressBar(int sizeIn, Options optionsIn, string labelIn)
        {
            _charsDrawn = 0;
            _label = labelIn;
            _progChar = DEFAULT_PROG_CHAR;
            _options = optionsIn;
            _size = sizeIn;
            PercentComplete = 0;

            WriteSetupProgBar();
        }

        /// <summary>
        /// Parameterized constructor for a custom sized progress bar set to a custom initial state
        /// </summary>
        /// <param name="sizeIn">Number of increments in the progress bar</param>
        /// <param name="initPercentage">Percentage complete when progress bar is first drawn</param>
        public ProgressBar(int sizeIn, uint initPercentage)
        {
            _charsDrawn = 0;
            _label = DEFAULT_LABEL;
            _options = DEFAULT_OPTIONS;
            _progChar = DEFAULT_PROG_CHAR;
            _size = sizeIn;
            PercentComplete = 0;

            WriteSetupProgBar();

            // Initialize the progress bar with the number of ticks indicated
            Tick((int)initPercentage);
        }

        /// <summary>
        /// Parameterized constructor accommodating custom progress bar size, initial percentage complete, display options,
        /// and a custom label
        /// </summary>
        /// <param name="sizeIn">Number of increments in the progress bar</param>
        /// <param name="initPercentage">Percentage complete when progress bar is first drawn</param>
        /// <param name="optionsIn">Options to control visual appearance of the progress bar</param>
        /// <param name="labelIn">Custom label to appear to the left of the progress bar</param>
        public ProgressBar(int sizeIn, uint initPercentage, Options optionsIn, string labelIn)
        {
            _charsDrawn = 0;
            _label = labelIn;
            _options = optionsIn;
            _progChar = DEFAULT_PROG_CHAR;
            _size = sizeIn;
            PercentComplete = 0;

            WriteSetupProgBar();

            // Initialize the progress bar with the number of ticks indicated
            Tick((int)initPercentage);
        }

        /// <summary>
        /// Options to control visual appearance of the progress bar
        /// </summary>
        [Flags]
        public enum Options
        {
            /// <summary>
            /// Display percentage complete to the right of the bar
            /// </summary>
            DisplayPercentage = 1,
            /// <summary>
            /// Display a custom text label to the left of the bar
            /// </summary>
            DisplayLabel = 2,
            /// <summary>
            /// Bookend the bar with square brackets
            /// </summary>
            Bookend = 4,
            /// <summary>
            /// Start the bar on the same line at the existing cursor current pos + 1 column
            /// </summary>
            StartSameLine = 8,
            /// <summary>
            /// When progress reaches 100%, remove the bar from the screen
            /// </summary>
            RemoveWhenDone = 16,
            /// <summary>
            /// Hides the cursor while drawing
            /// </summary>
            HideCursor = 32
        }

        /// <summary>
        /// Increments PercentComplete by 1 and updates the progress bar accordingly
        /// </summary>
        public void Tick()
        {
            if (!IsDone && PercentComplete < 100)
            {
                PercentComplete += 1;
                WriteIncremental();
            }
            if (!IsDone && PercentComplete == 100)
            {
                IsDone = true;
                WriteFinish();
            }
        }

        /// <summary>
        /// Increments PercentComplete by a specified value and updates the progress bar accordingly
        /// </summary>
        /// <param name="numTicks">Number of increments to perform</param>
        public void Tick(int numTicks)
        {
            for (int i = 0; i < numTicks; i++)
            {
                Tick();
            }
        }


        /// <summary>
        /// Writes the initialized state of the progress bar to the console and prepares cursor
        /// </summary>
        private void WriteSetupProgBar()
        {
            // Disable cursor
            if ((_options & Options.HideCursor) != 0)
            {
                Console.CursorVisible = false;
            }

            // Ensure cursor row position
            if ((_options & Options.StartSameLine) != 0)
            {
                Console.Write(' ');
            }

            // Mark progress bar start position
            _progBarStartPosX = Console.CursorLeft;
            _progBarStartPosY = Console.CursorTop;

            // Draw the label to the left of the bar
            if ((_options & Options.DisplayLabel) != 0)
            {
                Console.Write(_label + " ");
            }

            // Draw the left bookend character
            if ((_options & Options.Bookend) != 0)
            {
                Console.Write('[');
            }

            // Move cursor along the space for the bar to ensure it is empty
            int incrementalStartPosX = Console.CursorLeft;
            int incrementalStartPosY = Console.CursorTop;
            for (int i = 0; i < _size; i++)
            {
                Console.Write(' ');
            }

            // Draw the right bookend character
            if ((_options & Options.Bookend) != 0)
            {
                Console.Write(']');
            }

            // Setup the percentage label to the right of the bar
            if ((_options & Options.DisplayPercentage) != 0)
            {
                Console.Write("  ");
                _percentStartPosX = Console.CursorLeft;
                _percentStartPosY = Console.CursorTop;
                Console.Write("     %");
            }

            // Mark the end position
            _progBarEndPosX = Console.CursorLeft;
            _progBarEndPosY = Console.CursorTop;

            // Move the cursor back to the progress bar start position
            Console.SetCursorPosition(incrementalStartPosX, incrementalStartPosY);

            // Mark running progress bar tracker position
            _progBarToDrawPosX = Console.CursorLeft;
            _progBarToDrawPosY = Console.CursorTop;
        }

        /// <summary>
        /// Updates the progress bar and percentage display based on PercentageComplete value
        /// </summary>
        private void WriteIncremental()
        {
            if (Math.Floor(PercentComplete * (0.01 * _size)) > _charsDrawn)
            {
                // Draw a character on the progress bar
                Console.Write(_progChar);

                // Increment the char counter
                _charsDrawn += 1;

                // Mark running progress bar tracker position
                _progBarToDrawPosX = Console.CursorLeft;
                _progBarToDrawPosY = Console.CursorTop;
            }

            // Update percentage
            if ((_options & Options.DisplayPercentage) != 0)
            {
                Console.SetCursorPosition((int)_percentStartPosX, (int)_percentStartPosY);
                Console.Write(PercentComplete);
                Console.CursorLeft = _progBarToDrawPosX;
                Console.CursorTop = _progBarToDrawPosY;
            }
        }

        /// <summary>
        /// Finishes writing progress bar to the console
        /// </summary>
        private void WriteFinish()
        {
            if ((_options & Options.RemoveWhenDone) != 0)
            {
                Console.CursorLeft = _progBarStartPosX;
                Console.CursorTop = _progBarStartPosY;

                while (Console.CursorLeft != _progBarEndPosX || Console.CursorTop != _progBarEndPosY)
                {
                    Console.Write(' ');
                }

                Console.CursorLeft = _progBarStartPosX;
                Console.CursorTop = _progBarStartPosY;
            }

            if ((_options & Options.HideCursor) != 0)
            {
                Console.CursorVisible = true;
            }
        }
    }
}