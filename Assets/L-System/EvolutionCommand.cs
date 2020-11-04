public class EvolutionCommand{
	public readonly EvolutionState state;
	public readonly LSysContainer prevToLoad;
	public readonly LRule ruleToDraw;
	
	/// <summary>
	/// Initializes a new instance of the <see cref="EvolutionCommand"/> class.
	/// </summary>
	/// <param name="mating">If set to <c>true</c> mating.</param>
	/// <param name="loading">If set to <c>true</c> loading.</param>
	/// <param name="prevToLoad">Previous to load.</param>
	/// <param name="ruleToDraw">Rule to draw.</param>
	public EvolutionCommand(EvolutionState state, LRule ruleToDraw, LSysContainer prevToLoad){
		this.state = state;
		this.prevToLoad = prevToLoad;
		this.ruleToDraw = ruleToDraw;
	}
	
	public EvolutionCommand() : this(EvolutionState.NONE, null, null){}
}
