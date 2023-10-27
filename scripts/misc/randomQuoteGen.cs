using Godot;
using System;

public class randomQuoteGen : Label
{
    private string[] prefixes = {
    "The great",
    "The wise",
    "The powerful",
    "The enigmatic",
    "The legendary",
    "The fearless",
    "The arcane",
    "The mystical",
    "The valiant",
    "The ancient",
    "The benevolent",
    "The stoic",
    "The elusive"
};

    private string[] titles = {
    "wizard",
    "king",
    "warrior",
    "druid",
    "sorceress",
    "queen",
    "knight",
    "bard",
    "alchemist",
    "ranger",
    "seer",
    "necromancer",
    "paladin",
    "shaman"
};

    private string[] quotes = {
    "Guard your passwords like a dragon guards its gold.",
    "Let your secrets be as elusive as a phantom's shadow.",
    "Your password is the key to your digital kingdom.",
    "In the darkest forest, a hero emerged, wielding a sword of destiny.",
    "The castle's moat concealed secrets deeper than its waters.",
    "A knight's valor is tested in the heat of battle.",
    "Whispers of magic echoed through the ancient library's halls.",
    "Legends are born on the edge of a sword's blade.",
    "A dragon's roar shook the very foundations of the kingdom.",
    "The queen's wisdom guided the realm through treacherous times.",
    "In the shadow of the castle, a thief plotted daring heists.",
    "The wizard's staff crackled with untamed power.",
    "Knights rode forth to face the unknown, for honor and glory."
};

    public override void _Ready()
    {
        string prefix = prefixes[new Random().Next(prefixes.Length)];
        string title = titles[new Random().Next(titles.Length)];
        string quote = quotes[new Random().Next(quotes.Length)];

        string text = $"{prefix} {title} says: {quote}";
        Text = text;
    }
}