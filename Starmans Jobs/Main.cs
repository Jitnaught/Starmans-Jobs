using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using NativeUI;
using Newtonsoft.Json;
using Starmans_Jobs.Classes;

namespace Starmans_Jobs
{
    public class Main : Script
    {
        // Define all necessary lists
        private static List<Job> jobs = new List<Job>();
        private static List<Blip> blips = new List<Blip>();
        private static List<School> schools = new List<School>();
        private static List<object> courses = new List<object>();

        // Define all necessary file paths
        private static string modDirectory = "scripts\\StarmansJobs";
        private static string jobsDirectory = "scripts\\StarmansJobs\\jobs.json";
        private static string saveDirectory = "scripts\\StarmansJobs\\saveData.json";
        private static string schoolDirectory = "scripts\\StarmansJobs\\schools.json";
        // private static string configDirectory = "scripts\\StarmansJobs\\config.json"; ------> TO-DO IN VERSION: D2.0

        // Define all NativeUI variables
        private static MenuPool pool = new MenuPool();
        private static UIMenu schoolMenu = new UIMenu("School", "SELECT A COURSE");
        private static UIMenuListItem coursesList;
        private static UIMenuItem takeCourseBtn;

        // Define all necessary misc variables
        private static PlayerData currentPlayer;
        private static Save saveData;
        private static Job nearestJob;
        private static School nearestSchool;
        // private static ModConfig modConfig; ------> TO-DO IN VERSION: D2.0

        public Main()
        {
            // Run all functions that are to run on startup
            VerifyFileStructure();
            VerifyData();
            Setup();

            // Define all necessary functions to make the script 'tick'
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAbort;

            UI.Notify("Starmans Jobs Version ~y~D1.0 ~w~Loaded successfully"); // Notify the user that the mod has loaded
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.Player.Character.Model == new Model("player_two")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.trevor); // If you're playing as Trevor set the current player to be Trevors data
            else if (Game.Player.Character.Model == new Model("player_zero")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.michael); // If you're playing as Michael set the current player to be Michaels data
            else if (Game.Player.Character.Model == new Model("player_one")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.franklin); // If you're playing as Franklin set the current player to be Franklins data
            else currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.other); // If you're playing as a non story character set the current player to be the global playerdata

            if (pool != null) pool.ProcessMenus(); // Process all UI Menus if the menu pool is not null

            foreach (Job job in jobs) // Iterate over every job
            {
                Color markerColour = Color.Yellow; // Create a variable containing the colour of the 3d marker for the job
                if (job.degreeRequirement != Degree.none && !currentPlayer.degrees.Contains(job.degreeRequirement)) markerColour = Color.Red; // If the player is ineligible to interact with the job set the marker colour to red
                Vector3 jobLocation = new Vector3(job.location.X, job.location.Y, job.location.Z - 1); // Create a vector3 containing the location of the job and take 1 off the Z so the location is on the ground
                World.DrawMarker(MarkerType.VerticalCylinder, jobLocation, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), markerColour); // Draw a marker at the job
                if (World.GetDistance(Game.Player.Character.Position, new Vector3(jobLocation.X, jobLocation.Y, jobLocation.Z + 1)) <= .75f) // If the player is near the job
                {
                    nearestJob = job; // Set nearestJob to be the job the player is near
                    TimeSpan currentTime = World.CurrentDayTime; // Create a variable that contains the current day
                    WorkHours hours = job.positions.FirstOrDefault().hours; // Create a variable that contains the hours for this job
                    if (currentPlayer.job == job && currentTime.Hours == hours.startingHour && currentTime.Minutes >= hours.startingMinute) // If the player is working for this company and they are at the location within the first hour of work
                    {
                        UI.ShowSubtitle("Press ~y~E~w~ to start work", 1); // Notify the user they can start work with E
                    }
                    else if (currentPlayer.job != nearestJob) // If the player is not working for this company and the player has the right degree
                    {
                        if(nearestJob.degreeRequirement != Degree.none) // If the job needs a degree
                        {
                            if (currentPlayer.degrees.Contains(nearestJob.degreeRequirement)) UI.ShowSubtitle("Press ~y~E~w~ to apply at " + nearestJob.name, 1);  // If the player has the degree needed for the job then tell them they can apply
                            else UI.ShowSubtitle($"~r~You are ineligible to apply for this job. ~b~[Degree Required: ~y~{job.degreeRequirement.ToString()}~b~]", 1);
                        }
                        else if(nearestJob.degreeRequirement == Degree.none) UI.ShowSubtitle("Press ~y~E~w~ to apply at " + nearestJob.name, 1); // Notify the user they can apply at the job
                    }
                }
            }

            foreach (School school in schools) // Iterate over every school
            {
                Vector3 schoolLocation = new Vector3(school.location.X, school.location.Y, school.location.Z - 1); // Create a vector3 containing the location of the school and take 1 off the Z so the location is on the ground
                World.DrawMarker(MarkerType.VerticalCylinder, schoolLocation, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Color.Yellow); // Draw a marker at the school
                if (World.GetDistance(Game.Player.Character.Position, new Vector3(schoolLocation.X, schoolLocation.Y, schoolLocation.Z + 1)) <= .75f && !schoolMenu.Visible) // If the player is near the school
                {
                    nearestSchool = school; // Set nearestSchool to be the school the player is near
                    UI.ShowSubtitle($"Press ~y~E~w~ to view courses", 1); // Notify the user they can view courses
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if(e.KeyCode == Keys.E) // If E is pressed
            {
                foreach (School school in schools) // Iterate over every school
                {
                    Vector3 schoolLocation = new Vector3(school.location.X, school.location.Y, school.location.Z); // Create a new variable containing the schools location
                    if (World.GetDistance(Game.Player.Character.Position, new Vector3(schoolLocation.X, schoolLocation.Y, schoolLocation.Z)) <= .75f) // If the player is close to the school
                    {
                        schoolMenu.Visible = !schoolMenu.Visible;
                    }
                }

                Vector3 jobLocation = new Vector3(nearestJob.location.X, nearestJob.location.Y, nearestJob.location.Z); // Create a variable containing the location of the job
                TimeSpan currentTime = World.CurrentDayTime; // Create a variable that contains the current day
                WorkHours hours = nearestJob.positions.FirstOrDefault().hours; // Create a variable that contains the hours for this job

                if (currentPlayer.job == nearestJob && currentTime.Hours == hours.startingHour && currentTime.Minutes >= hours.startingMinute && World.GetDistance(Game.Player.Character.Position, jobLocation) <= .75f) // If the player is working for this company and they are at the location within the first hour of work
                {
                    Game.FadeScreenOut(1000); // Fade the screen out
                    Wait(1500); // Wait 1500ms
                    World.CurrentDayTime = new TimeSpan(hours.endingHour, hours.endingMinute, 0); // Set the time to the ending time of this job
                    Wait(1500); // Wait 1500ms
                    Game.FadeScreenIn(1000); // Fade the screen in 
                    EmployeePosition pos = currentPlayer.job.positions.FirstOrDefault(); // Create a variable to store the employee position of the current player
                    int multiple = 0; // Create a new int that will contain the number to multiply the pay by
                    // Get the difference between the starting and ending hours
                    if (pos.hours.startingHour > pos.hours.endingHour) multiple = pos.hours.startingHour - pos.hours.endingHour;
                    else multiple = pos.hours.endingHour - pos.hours.startingHour;

                    Game.Player.Money = Game.Player.Money + pos.hourlyPay * multiple; // Add the money to the user
                }
                else if(currentPlayer.job != nearestJob && World.GetDistance(Game.Player.Character.Position, jobLocation) <= .75f)
                {
                    Random rd = new Random(); // Create a new random
                    int n = rd.Next(0,100); // Use the random to generate a random number from 0 to 100
                    bool hired = false; // Create a bool to determine if the player has been hired
                    if (n >= 45) hired = true; // If the int is greater or equal to 45 then the player is hired
                    else if (n < 45) hired = false; // If the int is less than 45 then the player is not hired

                    Game.FadeScreenOut(1000); // Fade the screen out
                    Wait(1500); // Wait for 1500ms
                    World.CurrentDayTime = new TimeSpan(World.CurrentDayTime.Hours + 1, World.CurrentDayTime.Minutes, World.CurrentDayTime.Seconds); // Increase time by 1 hour
                    Wait(1500); // Wait for 1500ms
                    Game.FadeScreenIn(1000); // Fade the screen in
                    if (nearestJob.degreeRequirement != Degree.none && currentPlayer.degrees.Contains(nearestJob.degreeRequirement))
                    {
                        if (hired) // If the user is hired
                        {
                            UI.Notify($"You have been hired as a ~y~{nearestJob.positions.FirstOrDefault().name} ~w~for ~y~{nearestJob.name}~w~!"); // Notify the user about them being hired
                            currentPlayer.job = nearestJob; // Set the players job to nearest job
                            Save(); // Save the mod
                        }
                        else // If the user is not hired
                        {
                            UI.Notify($"Sorry, but you have not been hired by ~y~{nearestJob.name}"); // Notify the user that they have not been hired
                        }
                    }
                    else
                    {
                        if (hired) // If the user is hired
                        {
                            UI.Notify($"You have been hired as a ~y~{nearestJob.positions.FirstOrDefault().name} ~w~for ~y~{nearestJob.name}~w~!"); // Notify the user about them being hired
                            currentPlayer.job = nearestJob; // Set the players job to nearest job
                            Save(); // Save the mod
                        }
                        else // If the user is not hired
                        {
                            UI.Notify($"Sorry, but you have not been hired by ~y~{nearestJob.name}"); // Notify the user that they have not been hired
                        }
                    }
                }
            }
        }

        private void OnAbort(object sender, EventArgs e)
        {
            foreach(Blip blip in blips) // Iterate over all blips in the blips list
            {
                blip.Remove(); // Remove the blip
            }
        }

        private static void VerifyFileStructure()
        {
            // Check that all directories & files are present, if they aren't create and populate them (if a file is missing, this will not only create a new one but populate it with the default values)
            if (!Directory.Exists(modDirectory)) Directory.CreateDirectory(modDirectory);
            if (!File.Exists(jobsDirectory)) { string json = JsonConvert.SerializeObject(jobs); File.WriteAllText(jobsDirectory, json); }
            if (!File.Exists(saveDirectory)) { string json = JsonConvert.SerializeObject(saveData); File.WriteAllText(saveDirectory, json); }
            if (!File.Exists(schoolDirectory)) { string json = JsonConvert.SerializeObject(schools); File.WriteAllText(schoolDirectory, json); }
            // if (!File.Exists(configDirectory)) { string json = JsonConvert.SerializeObject(modConfig); File.WriteAllText(configDirectory, json); } ------> TO-DO IN VERSION: D2.0
        }

        private static void VerifyData()
        {
            // Ensure that the playerData in the currently loaded saveData and the save data itself is not nulled and that the playerData contains information on the current playermodel
            Load(); // Reload the json files
            if (saveData == null) saveData = new Save(); // If saveData is null, create a new save
            if (jobs == null) jobs = new List<Job>(); // if jobs is null, create a new list of jobs
            if (saveData.playerData == null)
            {
                // Make a new set of default player data and save it
                saveData.playerData = new List<PlayerData>();
                PlayerData franklin = new PlayerData()
                {
                    player = Classes.Player.franklin,
                    job = null,
                    degrees = new List<Degree>()
                };
                PlayerData michael = new PlayerData()
                {
                    player = Classes.Player.michael,
                    job = null,
                    degrees = new List<Degree>()
                };
                PlayerData trevor = new PlayerData()
                {
                    player = Classes.Player.trevor,
                    job = null,
                    degrees = new List<Degree>()
                };
                PlayerData other = new PlayerData()
                {
                    player = Classes.Player.other,
                    job = null,
                    degrees = new List<Degree>()
                };
                // Add the player data to the save
                saveData.playerData.Add(franklin);
                saveData.playerData.Add(michael);
                saveData.playerData.Add(trevor);
                saveData.playerData.Add(other);
            }
            Save(); // Save all data to json
        }

        private static void Save()
        {
            // Save the savedata to json files
            VerifyFileStructure(); // Verify all files

            // Serialize everything that is going to be saved and put the resulting string in variables
            string saveDataJson = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            string jobsJson = JsonConvert.SerializeObject(jobs, Formatting.Indented);
            string schoolJson = JsonConvert.SerializeObject(schools, Formatting.Indented);

            // Write all of the serialized strings to files
            File.WriteAllText(saveDirectory, saveDataJson);
            File.WriteAllText(jobsDirectory, jobsJson);
            File.WriteAllText(schoolDirectory, schoolJson);
        }

        private static void Load()
        {
            // Load the mod data from json and put it in a variables
            string saveDataJson = File.ReadAllText(saveDirectory);
            string jobsJson = File.ReadAllText(jobsDirectory);
            string schoolJson = File.ReadAllText(schoolDirectory);

            // Deserialize all mod data and put it in the appropriate variables
            saveData = JsonConvert.DeserializeObject<Save>(saveDataJson);
            jobs = JsonConvert.DeserializeObject<List<Job>>(jobsJson);
            schools = JsonConvert.DeserializeObject<List<School>>(schoolJson);

            if (Game.Player.Character.Model == new Model("player_two")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.trevor); // If you're playing as Trevor set the current player to be Trevors data
            else if (Game.Player.Character.Model == new Model("player_zero")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.michael); // If you're playing as Michael set the current player to be Michaels data
            else if (Game.Player.Character.Model == new Model("player_one")) currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.franklin); // If you're playing as Franklin set the current player to be Franklins data
            else currentPlayer = saveData.playerData.FirstOrDefault(x => x.player == Classes.Player.other); // If you're playing as a non story character set the current player to be the global playerdata
        }

        private static void Setup()
        {
            // Setup the courses list for the UI Menu
            courses.Add("Technology");
            courses.Add("Law");
            courses.Add("Flight");
            courses.Add("Acting");
            courses.Add("Medical");
            courses.Add("Engineering");

            // Setup the UI Menu itself
            coursesList = new UIMenuListItem("Course", courses, 1); // Set the coursesList variable to be a new UIMenuListItem named course that  uses the "courses" list as its list
            takeCourseBtn = new UIMenuItem("Take Course"); // Set the Take Course Button to be a new menu item called "Take Course"
            takeCourseBtn.Activated += ButtonActivated; // When take course is pressed
            schoolMenu.AddItem(coursesList); // Add the courses list to the school menu
            schoolMenu.AddItem(takeCourseBtn); // Add the take course button to the school menu
            pool.Add(schoolMenu); // Add the school menu to the menu pool

            foreach (Job job in jobs) // Iterate over every job
            {
                Vector3 jobLocation = new Vector3(job.location.X, job.location.Y, job.location.Z); // Create a new variable with the location of the job
                Blip blip = World.CreateBlip(jobLocation); // Draw a blip at the job
                blip.Sprite = BlipSprite.DollarSignCircled; // Set the sprite of the blip to be a circled dollar sign
                blip.Color = BlipColor.Yellow3; // Set the colour of the blip to be Yellow
                blip.Name = "Business"; // Name the blip "Business"
                blip.IsShortRange = true; // Set the blip to be short range
                blips.Add(blip); // Add the blip to the list of blips that are removed when the mod is aborted
            }

            foreach (School school in schools) // Iterate over every school
            {
                Vector3 schoolLocation = new Vector3(school.location.X, school.location.Y, school.location.Z); // Create a new variable with the location of the school
                Blip blip = World.CreateBlip(schoolLocation); // Create a new blip at the schools location
                blip.Sprite = BlipSprite.CaptureHouse; // Set the sprite of the blip to be 'CaptureHouse'
                blip.Color = BlipColor.YellowDark; // Set the colour of the blip to be a dark yellow
                blip.Name = "School"; // Name the blip "School"
                blip.IsShortRange = true; // Set the blip to be short range
                blips.Add(blip); // Add the blip to the list of blips that are removed when the mod is aborted
            }
        }

        private static void ButtonActivated(UIMenu sender, UIMenuItem selectedItem)
        {
            if(selectedItem.Text == takeCourseBtn.Text) // If the selected items text is equal to the take course text
            {
                Degree degreeToAdd; // Create a new variable that will contain the degree to add to the player

                // Check if the selected item is any valid course and if it is, set degreeToAdd to be the corrosponding degree. If it is not then return out of this function
                if (coursesList.CurrentItem().ToLower() == "technology") degreeToAdd = Degree.technology;
                else if (coursesList.CurrentItem().ToLower() == "law") degreeToAdd = Degree.law;
                else if (coursesList.CurrentItem().ToLower() == "flight") degreeToAdd = Degree.pilot;
                else if (coursesList.CurrentItem().ToLower() == "acting") degreeToAdd = Degree.acting;
                else if (coursesList.CurrentItem().ToLower() == "engineering") degreeToAdd = Degree.engineering;
                else if (coursesList.CurrentItem().ToLower() == "medical") degreeToAdd = Degree.medical;
                else return;

                if (!currentPlayer.degrees.Contains(degreeToAdd) && Game.Player.Money >= nearestSchool.pricePerCourse)
                {
                    schoolMenu.Visible = false; // Close the school menu
                    Game.FadeScreenOut(1000); // Fade the screen out
                    Wait(1500); // Wait for 1500 ms
                    World.CurrentDate = new DateTime(World.CurrentDate.Year + 1, World.CurrentDate.Month, World.CurrentDate.Day); // Change the current date to be a year in advance to simulate being at college/university
                    Wait(1500); // Wait for 1500 ms
                    Game.FadeScreenIn(1000); // Fade the screen in
                    currentPlayer.degrees.Add(degreeToAdd); // Add the degree to the player
                    Save(); // Save the mod
                    UI.Notify($"Congrats! You now have a degree in ~y~{coursesList.CurrentItem()}~w~!"); // Notify the user about their new degree
                }
                else if (Game.Player.Money < nearestSchool.pricePerCourse) UI.ShowSubtitle($@"~r~You cannot afford this course!
~b~[Your Money: ~y~${Game.Player.Money} ~b~Required Money: ~y~${nearestSchool.pricePerCourse}~b~]", 5000); // Notify the user about the fact they cannot afford this course
                else UI.ShowSubtitle("~r~You already have this degree!", 5000); // Notify the user about the fact they already have the selected degree
            }
        }
    }
}
