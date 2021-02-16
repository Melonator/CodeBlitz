using CodeBlitz.Levels;
using Godot;

namespace CodeBlitz
{
	public class WinScreen : Control
	{
		private string[] text = {"", "\nTeam", "\nWins"};
		private int _index = 0;
		public override void _Ready()
		{
			if(Troops.Winner == "Tie!")
			{
				text[0] = "It's";
				text[1] = "A";
				text[2] = Troops.Winner;
			} 

			else
				text[0] = Troops.Winner;
			GetNode<Timer>("Timer").Start();
			GetNode<AudioStreamPlayer>("WinPlayer").Play();
		}

		private void _on_Timer_timeout()
		{
			GetNode<AudioStreamPlayer>("TextPlayer").Play();
			GetNode<Label>("WinnerLabel").Text += text[_index];
			_index++;

			if(_index == 3)
				GetNode<Timer>("Timer").Stop();
			else    
				GetNode<Timer>("Timer").Start();
		}
	}
}
