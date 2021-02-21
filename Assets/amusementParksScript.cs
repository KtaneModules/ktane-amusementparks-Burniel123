using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class amusementParksScript : MonoBehaviour 
{
	public KMAudio Audio;
	public KMBombInfo Bomb;

	//Module components:
	public KMSelectable leftButton;
	public KMSelectable rightButton;
	public KMSelectable rideScreen;
	public Renderer banner;

	//Key module variables:
	private bool moduleSolved = false;
	private String[] parkNames = {"Cruelton Towers", "Dismay World", "Six Drags", "Cheddar Point", "Global Studios", "Fuji-Q ByeLand", "Trollywood"};
	private Fan[] fans = new Fan[] {new Fan("James", 3, 4, RideType.Water, 1), new Fan("Dan", 4, 3, RideType.RollerCoaster, 2), new Fan("Karen", 1, 1, RideType.SmallFlat, 3), new Fan("Susan", 2, 4, RideType.LargeFlat, 1),
									new Fan("Jessica", 4, 3, RideType.LargeFlat, 1), new Fan("Stephen", 2, 2, RideType.RollerCoaster, 2), new Fan("Matt", 1, 1, RideType.Water, 2), new Fan("Sarah", 3, 2, RideType.Dark, 3)};
	private Ride[] rides = new Ride[] {new Ride("Carousel", 1, new int[] {1}, RideType.SmallFlat, 2), new Ride("Drop Tower", 4, new int[] {4}, RideType.SmallFlat, 0), new Ride("Enterprise", 4, new int[] {4}, RideType.LargeFlat, 0), new Ride("Ferris Wheel", 2, new int[] {1,2,3,4}, RideType.LargeFlat, 1), new Ride("Ghost Train", 3, new int[] {3}, RideType.Dark, 3),
									   new Ride("Walkthrough", 1, new int[] {2}, RideType.Other, 2), new Ride("Inverted Coaster", 4, new int[] {4}, RideType.RollerCoaster, 2), new Ride("Junior Coaster", 2, new int[] {2}, RideType.RollerCoaster, 1), new Ride("Launched Coaster", 4, new int[] {3}, RideType.RollerCoaster, 0), new Ride("Log Flume", 2, new int[] {2}, RideType.Water, 2),
									   new Ride("Omnimover", 2, new int[] {1}, RideType.Dark, 3), new Ride("Pirate Ship", 4, new int[] {2}, RideType.LargeFlat, 2), new Ride("River Rapids", 3, new int[] {1,2,3,4}, RideType.Water, 3), new Ride("Safari", 3, new int[] {1,2,3,4}, RideType.Other, 1), new Ride("Star Flyer", 4, new int[] {3}, RideType.LargeFlat, 0),
									   new Ride("Top Spin", 4, new int[] {3}, RideType.LargeFlat, 2), new Ride("Tourbillon", 4, new int[] {3}, RideType.LargeFlat, 1), new Ride("Vintage Cars", 1, new int[] {1}, RideType.Other, 3), new Ride("Wooden Coaster", 3, new int[] {1,2,3,4}, RideType.RollerCoaster, 1)};

	private Ride[] ridesAvailable = new Ride[4];
	private String park = null;
	private int cycleIndex = 0;
	Ride correctInvestment = null;

	//Logging variables:
	static int moduleIdCounter = 1;
	int moduleId;

	//Awaken module - assign event handlers etc.
	void Awake()
	{
		moduleId = moduleIdCounter++;
		
		leftButton.OnInteract += delegate(){PressLeft(); return false;};
		rightButton.OnInteract += delegate(){PressRight(); return false;};
		rideScreen.OnInteract += delegate(){PressSubmit(); return false;};
	}

	//Initialize module.
	void Start() 
	{	
		int parkIndex = Enumerable.Range(0, 7).ToList().Shuffle()[0];
		park = parkNames[parkIndex];
		banner.GetComponentInChildren<TextMesh>().text = park;
		ridesAvailable = (new List<Ride>(rides)).Shuffle().Take(3).ToArray();
		Debug.LogFormat("[Amusement Parks #{0}] Welcome to {1}! It's time to decide what to invest in for next season!", moduleId, park);
		Debug.LogFormat("[Amusement Parks #{0}] The rides available for purchase are: {1}, {2}, {3}.", moduleId, ridesAvailable[0].name, ridesAvailable[1].name, ridesAvailable[2].name);
		rideScreen.GetComponentInChildren<TextMesh>().text = ridesAvailable[cycleIndex].name;
		Fan[] toConsult = DetermineFans();
		CalculatePoints(toConsult, ridesAvailable);
		correctInvestment = DetermineRide(park, ridesAvailable);
		Debug.LogFormat("[Amusement Parks #{0}] You should invest in the {1}.", moduleId, correctInvestment.name);
	}

	private Fan[] DetermineFans()
	{
		int pos1 = Bomb.GetBatteryCount() % 8;
		int pos2 = (pos1 + (Bomb.GetSerialNumberNumbers().Sum() % 7) + 1) % 8;
		int pos3 = (pos2 + (Bomb.GetSerialNumberNumbers().Sum() % 7) + 1) % 8;

		while(pos3 == pos1 || pos3 == pos2)
			pos3 = (pos3 + 1) % 8;

		Debug.LogFormat("[Amusement Parks #{0}] You should consult {1}, {2}, and {3}.", moduleId, fans[pos1].name, fans[pos2].name, fans[pos3].name);
		return new Fan[] {fans[pos1], fans[pos2], fans[pos3]};
	}

	private Ride[] CalculatePoints(Fan[] fans, Ride[] rides)
	{
		int serialNumSum = Bomb.GetSerialNumberNumbers().Sum();
		int numPlates = Bomb.GetPortPlates().Count();

		for(int i = 0; i < rides.Length; i++)
			rides[i].points = 0;

		for(int i = 0; i < 3; i++)
		{//Iterate each fan.
			int bioNumber = (new List<Fan>(this.fans)).IndexOf(fans[i]) + 1;
			
			int addAmt = serialNumSum % bioNumber == 0 ? numPlates : 1;
			int pointsAdded = 0;

			if(serialNumSum % bioNumber == 0)
			{
				addAmt = numPlates;
				Debug.LogFormat("[Amusement Parks #{0}] Serial digits rule applies for {1}!", moduleId, fans[i].name);
			}

			for(int j = 0; j < rides.Length; j++)
			{
				pointsAdded = 0;
				if(rides[j].thrillLevel == fans[i].prefThrill) {rides[j].points += addAmt; pointsAdded += addAmt;}
				if(Array.Exists(rides[j].suitableAges, e => e == fans[i].ageGroup)) {rides[j].points += addAmt; pointsAdded += addAmt;}
				if(rides[j].type == fans[i].prefType) {rides[j].points += addAmt; pointsAdded += addAmt;}
				if(rides[j].scenery == fans[i].prefScenery) {rides[j].points += addAmt; pointsAdded += addAmt;}
				Debug.LogFormat("[Amusement Parks #{0}] {1} gives {2} points to the {3}.", moduleId, fans[i].name, pointsAdded, rides[j].name);
			}
		}

		for(int i = 0; i < rides.Length; i++)
			Debug.LogFormat("[Amusement Parks #{0}] Total points for {1}: {2}.", moduleId, rides[i].name, rides[i].points);
		
		return rides;
	}

	private Ride DetermineRide(String park, Ride[] rides)
	{
		Ride[] sorted = (new List<Ride>(rides)).OrderBy(r => r.points).ThenByDescending(r => r.name).Reverse().ToArray(); //TODO alphabetical on ties.

		if(park.Equals("Cruelton Towers"))
		{
			for(int i = 0; i < sorted.Length; i++)
				if(!(new String[] {"Drop Tower", "Ferris Wheel", "Star Flyer"}).Contains(sorted[i].name))
					return sorted[i];
				else Debug.LogFormat("[Amusement Parks #{0}] Due to park-based restrictions, you cannot install the {1}.", moduleId, sorted[i].name);
		}
		
		if(park.Equals("Dismay World"))
		{
			for(int i = 0; i < sorted.Length; i++)
				if(!sorted[i].suitableAges.Equals(new int[] {4}))
					return sorted[i];
				else Debug.LogFormat("[Amusement Parks #{0}] Due to park-based restrictions, you cannot install the {1}.", moduleId, sorted[i].name);
		}

		if(park.Equals("Six Drags") && (Bomb.GetPortCount() >= 6 || Bomb.GetBatteryCount() >= 6))
		{
			for(int i = 0; i < sorted.Length; i++)
				if(sorted[i].scenery != 3)
					return sorted[i];
				else Debug.LogFormat("[Amusement Parks #{0}] Due to park-based restrictions, you cannot install the {1}.", moduleId, sorted[i].name);
		}

		if(park.Equals("Cheddar Point") && (Bomb.GetSerialNumberLetters().Any(x => x == 'F' || x == 'A' || x == 'I' || x == 'R')))
		{
			for(int i = 0; i < sorted.Length; i++)
				if(!(new String[] {"Safari", "Log Flume"}).Contains(sorted[i].name))
					return sorted[i];
				else Debug.LogFormat("[Amusement Parks #{0}] Due to park-based restrictions, you cannot install the {1}.", moduleId, sorted[i].name);
		}

		if(park.Equals("Not Berry Farm"))
		{
			for(int i = 0; i < sorted.Length; i++)
				if(sorted[i].type != RideType.RollerCoaster)
					return sorted[i];
				else Debug.LogFormat("[Amusement Parks #{0}] Due to park-based restrictions, you cannot install the {1}.", moduleId, sorted[i].name);
		}

		return sorted[0];
	}

	private void PressLeft()
	{
		if(!moduleSolved)
		{
			leftButton.AddInteractionPunch(0.5f);
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			cycleIndex = cycleIndex == 0 ? 2 : (cycleIndex - 1) % 3;
			rideScreen.GetComponentInChildren<TextMesh>().text = ridesAvailable[cycleIndex].name;
		}
		
	}

	private void PressRight()
	{
		if(!moduleSolved)
		{
			rightButton.AddInteractionPunch(0.5f);
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			cycleIndex = (cycleIndex + 1) % 3;
			rideScreen.GetComponentInChildren<TextMesh>().text = ridesAvailable[cycleIndex].name;
		}
		
	}

	private void PressSubmit()
	{
		if(!moduleSolved)
		{
			rideScreen.AddInteractionPunch(0.5f);
			String submitted = rideScreen.GetComponentInChildren<TextMesh>().text;
			if(submitted.Equals(correctInvestment.name))
			{
				Debug.LogFormat("[Amusement Parks #{0}] You invested in the {1}, which was correct. Module solved.", moduleId, submitted);
				GetComponent<KMBombModule>().HandlePass();
				Audio.PlaySoundAtTransform("scream", transform);
				moduleSolved = true;
			}
			else
			{
				GetComponent<KMBombModule>().HandleStrike();
				Debug.LogFormat("[Amusement Parks #{0}] You invested in the {1}, which was incorrect. Strike!", moduleId, submitted);
				Start();
			}
		}
		
	}

	#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Move left/right through available attractions with “!{0} press left/right/l/r”. Cycle all attractions with “!{0} cycle”. Submit with “!{0} submit” (submits current attraction) or “!{0} submit x” (submits the attraction named x).";
	#pragma warning restore 414
	//Process command for Twitch Plays.
	IEnumerator ProcessTwitchCommand(String command)
	{
		//var play = Regex.Match(command,@"^(\s)*(play){1}(\s)*$", RegexOptions.IgnoreCase);
		//var set = Regex.Match(command,@"^\s*(submit|((set\s([0-1]){5})(\ssubmit)?))(\s)*$", RegexOptions.IgnoreCase);
		var left = Regex.Match(command,@"^\s*(press)\s*(l(eft)?)\s*$", RegexOptions.IgnoreCase);
		var right = Regex.Match(command,@"^\s*(press)\s*(r(ight)?)\s*$", RegexOptions.IgnoreCase);
		var cycle = Regex.Match(command,@"^\s*(cycle)\s*$", RegexOptions.IgnoreCase);
		var submit = Regex.Match(command,@"^\s*(submit)(\s+([a-z]+))*\s*$", RegexOptions.IgnoreCase);

		//if(!(left.Success || right.Success || cycle.Success || submit.Success))
		//	yield break;
		
		if(left.Success)
		{
			yield return null;
			yield return leftButton;
		}
		else if(right.Success)
		{
			yield return null;
			yield return rightButton;
		}
		else if(cycle.Success)
		{
			yield return null;
			for(int i = 0; i < 3; i++)
			{
				yield return "trycancel";
				yield return new WaitForSeconds(1.25f);
				yield return rightButton;
				yield return rightButton;
			}
		}
		else if(submit.Success)
		{
			if(command.ToLowerInvariant().Trim().Equals("submit"))
			{
				yield return null;
				yield return rideScreen;
			}
			else
			{
				String submitted = command.ToLowerInvariant().Trim().Substring(6).Trim();
				Debug.Log(submitted);
				for(int i = 0; i < 3; i++)
				{
					Debug.Log(rideScreen.GetComponentInChildren<TextMesh>().text.ToLowerInvariant());
					if(rideScreen.GetComponentInChildren<TextMesh>().text.ToLowerInvariant().Equals(submitted))
					{
						yield return null;
						yield return rideScreen;
						break;
					}
					else
					{
						yield return null;
						yield return rightButton;
						yield return rightButton;
						//yield return new WaitForSeconds(1f);
						//if(i == 2) yield return "sendtochaterror {no such attraction available!}";
					}
				}
			}

			//String submitted = submit.Groups[].Value.ToLowerInvariant();
			//Debug.Log(submitted);
		}

		yield break;
	}
/*
		else if(command.ToLower().Contains("set"))
		{
			String valuesEntered = set.Groups[3].Value.ToLowerInvariant().Trim().Substring(4);
			for(int i = 0; i < valuesEntered.Length; i++)
			{
				if(valuesEntered[i] == '1' && bitDisplays[i].GetComponentInChildren<TextMesh>().text.Contains("0"))
				{
					yield return null;
					upArrows[i].OnInteract();
					yield return new WaitForSeconds(.05f);
				}
				else if(valuesEntered[i] == '0' && bitDisplays[i].GetComponentInChildren<TextMesh>().text.Contains("1"))
				{
					yield return null;
					downArrows[i].OnInteract();
					yield return new WaitForSeconds(.05f);
				}
			}
		}

		if(command.ToLower().Contains("submit"))
		{
			yield return null;
			submitButton.OnInteract();
		}
	}

	//Calls a coroutine to autosolve the module when a TP admin does !<id> solve.
	void TwitchHandleForcedSolve()
	{
		if(moduleSolved) return;
		StartCoroutine(HandleForcedSolve());
	}

	IEnumerator HandleForcedSolve()
	{
		yield return null;
		Debug.Log(solution);
		for(int i = 0; i < solution.Length; i++)
		{
			if(solution[i] == '1')
			{
				upArrows[i].OnInteract();
				yield return new WaitForSeconds(0.2f);
			}
			else
			{
				downArrows[i].OnInteract();
				yield return new WaitForSeconds(0.2f);
			}
		}

		submitButton.OnInteract();
	}*/
}

class Fan
{
	public String name;
	public int prefThrill;
	public int ageGroup;
	public RideType prefType;
	public int prefScenery;

	public Fan(String name, int prefThrill, int ageGroup, RideType prefType, int prefScenery)
	{
		this.name = name;
		this.prefThrill = prefThrill;
		this.ageGroup = ageGroup;
		this.prefType = prefType;
		this.prefScenery = prefScenery;
	}
}
class Ride
{
	public String name;
	public int thrillLevel;
	public int[] suitableAges;
	public RideType type;
	public int scenery;

	public int points;

	public Ride(String name, int thrillLevel, int[] suitableAges, RideType type, int scenery)
	{
		this.name = name;
		this.thrillLevel = thrillLevel;
		this.suitableAges = suitableAges;
		this.type = type;
		this.scenery = scenery;
		points = 0;
	}
}

enum RideType
{
	RollerCoaster, SmallFlat, LargeFlat, Water, Dark, Other
}