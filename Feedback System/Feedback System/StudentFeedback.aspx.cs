using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Feedback_System
{
    public partial class StudentFeedback : System.Web.UI.Page
    {

        private const int TotalQuestions = 10;//      >>>>>>>
        private string[] questions;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //OptionsList.CssClass = "hidden";
                //submit.CssClass = "hidden";
                //Button2.CssClass = "hidden";

                // Read student email from log.txt
                string logPath = Server.MapPath("~/App_Data/log.txt");
                string[] logLines = File.ReadAllLines(logPath);
                string[] logFields = logLines[0].Split(';');
                string targetEmail = logFields[1].Trim();
                Session["StudentEmail"] = targetEmail;

                // Load courses for the student
                string filePath = Server.MapPath("~/App_Data/Student_List.txt");
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    string email = fields[2].Trim();

                    if (email.Equals(targetEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] courses = fields.Skip(3).ToArray();
                        string[] coursrs_splited = courses[0].Split(',');

                        foreach (string course in coursrs_splited)
                        {
                            validationCustom04.Items.Add(new ListItem(course.Trim(), course.Trim()));
                        }
                        break;
                    }
                }
            }
        }

        protected void validationCustom04_SelectedIndexChanged(object sender, EventArgs e)
        {
            //    OptionsList.CssClass = "";
            //    Button2.CssClass = "btn btn-primary";
            //    submit.CssClass = "hidden";

            string selectedValue = validationCustom04.SelectedValue.Trim();
            string question_file = $"{selectedValue}_questions.txt";
            string file = Server.MapPath($"~/App_Data/Courses_Questions/{question_file}");



            try
            {
                if (File.Exists(file))
                {
                    questions = File.ReadAllLines(file);
                    Session["Questions"] = questions;
                    Session["QuestionNumber"] = 1;
                    Session["Feedback"] = new string[TotalQuestions];

                    DisplayCurrentQuestion();
                }
                else
                {
                    lblQuestion.Text = "Questions file not found.";
                }
            }
            catch (Exception ex)
            {
                lblQuestion.Text = $"Error: {ex.Message}";
            }
        }

        private void DisplayCurrentQuestion()
        {
            int questionNumber = Convert.ToInt32(Session["QuestionNumber"]);
            questions = (string[])Session["Questions"];

            if (questionNumber <= questions.Length)
            {
                string[] questionParts = questions[questionNumber - 1].Split(';');
                lblQuestion.Text = questionParts.Length > 1 ? questionParts[1].Trim() : "Question format incorrect.";
            }
            else
            {
                lblQuestion.Text = "No more questions.";
                //OptionsList.CssClass = "hidden";
                //Button2.CssClass = "hidden";
                //submit.CssClass = "btn btn-primary";
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OptionsList.SelectedValue))
            {
                int questionNumber = Convert.ToInt32(Session["QuestionNumber"]);
                string[] feedback = (string[])Session["Feedback"];
                feedback[questionNumber - 1] = OptionsList.SelectedValue;

                Session["Feedback"] = feedback;
                Session["QuestionNumber"] = questionNumber + 1;

                DisplayCurrentQuestion();
                OptionsList.ClearSelection();
            }
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            string selectedCourse = validationCustom04.SelectedValue.Trim();
            string studentEmail = Session["StudentEmail"].ToString();

            // Fetch student ID from Student_List.txt using email
            string studentId = GetStudentIdByEmail(studentEmail);
            string[] feedback = (string[])Session["Feedback"];

            // Construct feedback entry for all questions
            string feedbackEntry = $"{studentId};";
            for (int i = 0; i < feedback.Length; i++)
            {
                feedbackEntry += $"Q{i + 1}:{feedback[i]},";
            }
            feedbackEntry = feedbackEntry.TrimEnd(',') + ";";

            // Append feedback to course feedback file
            string feedbackFilePath = Server.MapPath($"~/App_Data/Courses_Feedbacks/{selectedCourse}.txt");

            using (StreamWriter sw = File.AppendText(feedbackFilePath))
            {
                sw.WriteLine(feedbackEntry);
            }

            lblQuestion.Text = "Feedback submitted successfully!";
            //    OptionsList.ClearSelection();
            //    OptionsList.CssClass = "hidden";
            //    submit.CssClass = "hidden";
            //    Button2.CssClass = "hidden";
            //}

            string GetStudentIdByEmail(string email)
            {
                string filePath = Server.MapPath("~/App_Data/Student_List.txt");
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(';');
                    if (fields[2].Trim().Equals(email, StringComparison.OrdinalIgnoreCase))
                    {
                        return fields[0].Trim(); // Assuming student ID is the first field
                    }
                }
                return string.Empty;
            }

        }

        protected void OptionsList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}