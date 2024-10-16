namespace Twksqr.Games.WordleSimulator;

using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

public static class GameManager
{
    public static readonly ReadOnlyCollection<string> AllWords;
    public static readonly ReadOnlyCollection<string> AllowedWords;
    public static readonly ReadOnlyCollection<string> BannedWords;
    public static readonly ReadOnlyCollection<string> CommonWords;

    public static readonly string ProjectFilePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

    public static readonly string AllWordsFilePath = @$"{ProjectFilePath}/data/all_words.json";
    public static readonly string AllowedWordsFilePath = @$"{ProjectFilePath}/data/allowed_words.json";
    public static readonly string BannedWordsFilePath = @$"{ProjectFilePath}/data/banned_words.json";
    public static readonly string CommonWordsFilePath = @$"{ProjectFilePath}/data/common_words.json";

    static GameManager()
    {
        AllWords = JsonConvert.DeserializeObject<ReadOnlyCollection<string>>(File.ReadAllText(AllWordsFilePath)) ?? new List<string>().AsReadOnly();
        AllowedWords = JsonConvert.DeserializeObject<ReadOnlyCollection<string>>(File.ReadAllText(AllowedWordsFilePath)) ?? new List<string>().AsReadOnly();
        BannedWords = JsonConvert.DeserializeObject<ReadOnlyCollection<string>>(File.ReadAllText(BannedWordsFilePath)) ?? new List<string>().AsReadOnly();
        CommonWords = JsonConvert.DeserializeObject<ReadOnlyCollection<string>>(File.ReadAllText(CommonWordsFilePath)) ?? new List<string>().AsReadOnly();
    }

    public static void Execute()
    {
        // TODO: Use Spectre.Console for UI, but we might need the char/password verifying thing from the bank account demo for character validation anyway

        AnsiConsole.MarkupLine("[blue]C# Wordle Simulator[/]");

        // ConsoleKeyInfo keyInfo = Console.ReadKey();

        // PlayGame(keyInfo.Key == ConsoleKey.Oem3);
        PlayGame(true);
    }

    // TODO: add replay functionality 
    static bool PlayGame(bool debugModeIsEnabled)
    {
        string answer;

        if (debugModeIsEnabled)
        {
            do 
            {
                Console.WriteLine("Input debug word:");

                answer = Console.ReadLine() ?? string.Empty;

                if (AllWords.Contains(answer))
                {
                    break;
                }

                Console.WriteLine("Invalid input.");
                // TODO: Inline invalid input message

            }
            while (true);
        }
        else
        {
            var rand = new Random(DateTime.Now.Millisecond);

            answer = AllowedWords[rand.Next(0, AllowedWords.Count)];
        }

        var guessResults = new List<GuessResult[]>();

        AnsiConsole.MarkupLine("[gray](Enter guess to start)[/]");

        do
        {
            string guess = ReadLineValidated() ?? string.Empty;

            if (guess.Length != 5 || !AllWords.Contains(guess))
            {
                Console.WriteLine("Invalid input.");
                continue;
            }

            var guessResult = new GuessResult[5];
            guessResults.Add(new GuessResult[5]);

            for (int i = 0; i < 5; i++)
            {
                if (guess[i] == answer[i])
                {
                    guessResult[i] = GuessResult.Correct;
                    continue;
                }

                for (int j = 0; j < 5; j++)
                {
                    if ((guess[j] == answer[i]) && guessResults[guessResults.Count - 1][j] == GuessResult.Incorrect)
                    {
                        guessResult[j] = GuessResult.WrongPosition;
                        break;
                    }
                }
            }

            Console.CursorTop--;

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(250);

                string resultColor = guessResult[i] switch
                {
                    GuessResult.Incorrect => "white on #3a3a3c",
                    GuessResult.WrongPosition => "white on #b59f3a",
                    GuessResult.Correct => "white on #548c4d",
                    _ => ""
                };

                AnsiConsole.Markup($"[{resultColor}]{char.ToUpper(guess[i])}[/]");
            }

            Console.ResetColor();

            Thread.Sleep(250);
            Console.WriteLine();

            if (guess == answer)
            {
                AnsiConsole.Markup($"[blue]Guesses: {guessResults.Count}[/]");

                Console.ReadKey();
                return true;
            }
        }
        while (guessResults.Count < 6);

        Console.WriteLine($"The word was \"{answer}\"");
        Console.ReadKey();
        return false;
    }

    public static string ReadLineValidated()  
    {  
        var input = new StringBuilder();
        ConsoleKeyInfo keyInfo;

        do  
        {
            keyInfo = Console.ReadKey(true);

            if ((keyInfo.Key == ConsoleKey.Backspace) && (input.Length > 0))  
            {
                input = input.Remove(input. Length - 1, 1);
                
                Console.Write("\b \b"); // Thanks John Arlen for the explanation!
                                        // https://stackoverflow.com/a/5195807/22315071

                continue;
            }
            if (!Regex.IsMatch(keyInfo.KeyChar.ToString(), @"\w"))
            {
                continue;
            }
            if (input.Length >= 5)
            {
                input.Remove(5, input.Length - 5);
                continue;
            }

            input.Append(keyInfo.KeyChar);
            Console.Write(char.ToUpper(keyInfo.KeyChar));
        }
        while (keyInfo.Key != ConsoleKey.Enter);

        Console.WriteLine();

        return input.ToString();
    }
}