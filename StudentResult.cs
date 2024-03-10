using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.pdf;
using iTextSharp.text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ProjectB
{
    public partial class StudentResult : UserControl
    {
        public StudentResult()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            // Validate that the Id textbox is empty
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()))
            {
                MessageBox.Show("Invalid Id. The Id textbox should be empty for a new entry.");
                return; // Exit the method if validation fails
            }
            // Validate RubricId as Integer
            if (!int.TryParse(textBox2.Text, out int AssessmentComponentId))
            {
                MessageBox.Show("Invalid AssessmentComponentId format. Please enter a valid integer for the AssessmentComponentId");
                return; // Exit the method if validation fails
            }
            // Validate MeasurementLevel as Integer
            if (!int.TryParse(textBox3.Text, out int RubricMeasurementId))
            {
                MessageBox.Show("Invalid RubricMeasurementId format. Please enter a valid integer for the RubricMeasurementId.");
                return; // Exit the method if validation fails
            }
            SqlCommand cmd = new SqlCommand("INSERT INTO [dbo].[StudentResult] VALUES (@AssessmentComponentId, @RubricMeasurementId, @EvaluationDate)", con);
            cmd.Parameters.AddWithValue("@AssessmentComponentId", AssessmentComponentId);
            cmd.Parameters.AddWithValue("@RubricMeasurementId", RubricMeasurementId);
            cmd.Parameters.AddWithValue("@EvaluationDate", dateTimePicker1.Value);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Successfully saved");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select * from dbo.StudentResult", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            // Validate Id as Integer
            if (!int.TryParse(textBox1.Text, out int id))
            {
                MessageBox.Show("Invalid Id format. Please enter a valid integer for the Id.");
                return; // Exit the method if validation fails
            }
            // Validate RubricId as Integer
            if (!int.TryParse(textBox2.Text, out int AssessmentComponentId))
            {
                MessageBox.Show("Invalid AssessmentComponentId format. Please enter a valid integer for the AssessmentComponentId.");
                return; // Exit the method if validation fails
            }
            // Validate MeasurementLevel as Integer
            if (!int.TryParse(textBox3.Text, out int RubricMeasurementId))
            {
                MessageBox.Show("Invalid RubricMeasurementId format. Please enter a valid integer for the RubricMeasurementId.");
                return; // Exit the method if validation fails
            }
            SqlCommand cmd = new SqlCommand("UPDATE StudentResult SET AssessmentComponentId=@AssessmentComponentId, RubricMeasurementId=@RubricMeasurementId, EvaluationDate=@EvaluationDate WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@AssessmentComponentId", AssessmentComponentId);
            cmd.Parameters.AddWithValue("@RubricMeasurementId", RubricMeasurementId);
            cmd.Parameters.AddWithValue("@EvaluationDate", dateTimePicker1);
            cmd.Parameters.AddWithValue("@Id", id);

            cmd.ExecuteNonQuery();
            MessageBox.Show("Successfully updated");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            if (!int.TryParse(textBox1.Text, out int id))
            {
                MessageBox.Show("Invalid Id format. Please enter a valid integer for the Id.");
                return; // Exit the method if validation fails
            }
            // Get the current maximum identity value before deleting
            SqlCommand getMaxIdCmd = new SqlCommand("SELECT MAX(Id) FROM StudentResult", con);
            int maxIdBeforeDelete = Convert.ToInt32(getMaxIdCmd.ExecuteScalar());
            SqlCommand deleteCmd = new SqlCommand("DELETE FROM StudentResult WHERE Id = @Id", con);
            deleteCmd.Parameters.AddWithValue("@Id", int.Parse(textBox1.Text));
            deleteCmd.ExecuteNonQuery();
            // Get the new maximum identity value after deleting
            int maxIdAfterDelete = maxIdBeforeDelete - 1;
            // Reset the identity column to the new maximum value
            SqlCommand resetIdentityCmd = new SqlCommand($"DBCC CHECKIDENT ('StudentResult', RESEED, {maxIdAfterDelete})", con);
            resetIdentityCmd.ExecuteNonQuery();
            MessageBox.Show("Successfully deleted and reset identity");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            if (!int.TryParse(textBox1.Text, out int id))
            {
                MessageBox.Show("Invalid Id format. Please enter a valid integer for the Id.");
                return; // Exit the method if validation fails
            }
            SqlCommand cmd = new SqlCommand("Select StudentId, AssessmentComponentId, RubricMeasurementId, EvaluationDate FROM StudentResult WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", int.Parse(textBox1.Text));
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            try
            {
                // Use a using statement to ensure proper disposal of SqlConnection
                using (SqlConnection connection = new SqlConnection(con.ConnectionString))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT s.Id AS StudentId, s.FirstName + ' ' + s.LastName AS studentName, c.Name AS CLOName, a.Title AS AssessmentTitle, ac.Name AS ComponentName, ac.TotalMarks AS ComponentTotalMarks, rl.MeasurementLevel AS Marks, (rl.MeasurementLevel / (SELECT MAX(MeasurementLevel) FROM RubricLevel WHERE RubricLevel.RubricId = ac.Id)) * ac.TotalMarks AS ObtainedMarks FROM Student s JOIN StudentResult sr ON s.Id = sr.StudentId JOIN AssessmentComponent ac ON sr.AssessmentComponentId = ac.Id JOIN RubricLevel rl ON sr.RubricMeasurementId = rl.Id JOIN Rubric r ON rl.RubricId = r.Id JOIN Clo c ON r.CloId = c.Id JOIN Assessment a ON ac.AssessmentId = a.Id WHERE s.Status = 5 ORDER BY s.Id, c.Id, a.Id, ac.Id, r.Id, rl.Id", connection);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }

            //var con = Configuration.getInstance().getConnection();
            //SqlCommand cmd = new SqlCommand("SELECT s.Id AS StudentId, Concat(s.FirstName, s.LastName) AS studentName,c.Name AS CLOName, a.Title AS AssessmentTitle,ac.Name AS ComponentName,ac.TotalMarks AS ComponentTotalMarks, rl.MeasurementLevel AS Marks, (rl.MeasurementLevel / (SELECT MAX(MeasurementLevel) FROM RubricLevel WHERE RubricLevel.RubricId = ac.Id)) *ac.TotalMarks AS ObtainedMarks FROM Student s JOIN StudentResult sr ON s.Id = sr.StudentId JOIN AssessmentComponent ac ON sr.AssessmentComponentId = ac.Id JOIN RubricLevel rl ON sr.RubricMeasurementId = rl.Id JOIN Rubric r ON rl.RubricId = r.Id JOIN Clo c ON r.CloId = c.Id JOIN Assessment a ON ac.AssessmentId = a.Id WHERE s.Status = 5 ORDER BY s.Id, c.Id, a.Id, ac.Id, r.Id, rl.Id", con);
            //SqlDataAdapter da = new SqlDataAdapter(cmd);
            //DataTable dt = new DataTable();
            //da.Fill(dt);
            //dataGridView1.DataSource = dt;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Student.Id AS StudentId,Concat(Student.FirstName, Student.LastName) AS StudentName,AssessmentComponent.Name AS AssessmentComponentName,RubricLevel.Id AS RubricLevelId,CASE WHEN RubricLevel.MeasurementLevel >= 4THEN 'A'  WHEN RubricLevel.MeasurementLevel >= 3 THEN 'B' WHEN RubricLevel.MeasurementLevel >= 2 THEN 'C'  WHEN RubricLevel.MeasurementLevel >= 1THEN 'D'  ELSE 'F' END AS Grade FROM Student INNER JOIN StudentResult ON Student.Id = StudentResult.StudentId  INNER JOIN AssessmentComponent ON StudentResult.AssessmentComponentId = AssessmentComponent.Id INNER JOIN RubricLevel ON StudentResult.RubricMeasurementId = RubricLevel.Id WHERE AssessmentComponent.AssessmentId = AssessmentId ORDER BY Student.Id, AssessmentComponent.Id, RubricLevel.Id", con);

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;
        }
        

        private void button8_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT StudentResult.StudentId, AssessmentComponent.AssessmentId, Assessment.TotalMarks, AVG(RubricLevel.MeasurementLevel) / MAX(RubricLevel.MeasurementLevel) * AssessmentComponent.TotalMarks AS ObtainedMarks, (CASE WHEN AVG(RubricLevel.MeasurementLevel) / MAX(RubricLevel.MeasurementLevel) * AssessmentComponent.TotalMarks >= 60 THEN 'A' WHEN AVG(RubricLevel.MeasurementLevel) / MAX(RubricLevel.MeasurementLevel) * AssessmentComponent.TotalMarks >= 40 THEN 'B' WHEN AVG(RubricLevel.MeasurementLevel) / MAX(RubricLevel.MeasurementLevel) * AssessmentComponent.TotalMarks >= 30 THEN 'C' WHEN AVG(RubricLevel.MeasurementLevel) / MAX(RubricLevel.MeasurementLevel) * AssessmentComponent.TotalMarks >= 20 THEN 'D'ELSE 'F' END) AS Grade FROM StudentResult INNER JOIN RubricLevel ON StudentResult.RubricMeasurementId = RubricLevel.Id INNER JOIN AssessmentComponent ON StudentResult.AssessmentComponentId = AssessmentComponent.Id Join Assessment On AssessmentComponent.Id = Assessment.Id GROUP BY StudentResult.StudentId, AssessmentComponent.AssessmentId, AssessmentComponent.TotalMarks, Assessment.TotalMarks", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            dataGridView1.DataSource = dt;
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            try
            {
    
                    con.Open();

                    string query = @"
                        SELECT
                            sr.StudentId,
                            ac.ComponentName,
                            r.Details AS Rubric,
                            ac.TotalMarks AS ComponentMarks,
                            rl.MeasurementLevel AS StudentLevel,
                            (rl.MeasurementLevel / (SELECT MAX(MeasurementLevel) FROM RubricLevel WHERE RubricLevel.RubricId = ac.RubricId)) * ac.TotalMarks AS ObtainedMarks
                        FROM
                            StudentResult sr
                        JOIN
                            AssessmentComponent ac ON sr.AssessmentComponentId = ac.Id
                        JOIN
                            RubricLevel rl ON sr.RubricMeasurementId = rl.Id
                        JOIN
                            Rubric r ON rl.RubricId = r.Id;
                    ";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            System.Data.DataTable dt = new System.Data.DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                //con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                // Ensure the connection is closed, even in case of an exception
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private void StudentResult_Load(object sender, EventArgs e)
        {

        }
    }
}
