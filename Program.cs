using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Fetching...");
            Console.WriteLine("Type h for help");

            XDocument rss = XDocument.Load(@"http://www.reddit.com/.rss");
            List<XElement> stories = rss.Descendants("item").ToList<XElement>();
            int oldindex = 1;
            int hackindex = 1;
            int index = 1;

            bool viewingComments = false;

            List<XElement> comments = new List<XElement>();
            int index_c = 1;

            string[] input = new string[] {"n"};

            while (input[0] != "q" && input[0] != "exit")
            {
                if (input[0] == "")
                {
                    input[0] = "n";
                }
                if (input[0] == "s")
                {
                        viewingComments = false;
                        index = hackindex;
                        input[0] = "n";

                }
                if (input[0] == "n")
                {
                    if (!viewingComments)
                    {
                        oldindex = hackindex;
                        hackindex = index;  // lol @ hack
                        index = displayHeadlines(index, stories, rss.Root.Element("channel").Element("title").Value);
                    }
                    else
                    {
                        index_c = displayComments(index_c, comments);
                    }
                }

                if (input[0] == "p")
                {
                    index = displayHeadlines(oldindex, stories, rss.Root.Element("channel").Element("title").Value);
                }

                if (input[0] == "r")
                {
                    if (input.Length == 2)
                    {
                        Console.WriteLine("Fetching " + input[1] + "...");
                        try
                        {
                            rss = XDocument.Load(@"http://www.reddit.com/r/" + input[1] + "/.rss");
                            stories = rss.Descendants("item").ToList<XElement>();
                            viewingComments = false;
                            oldindex = 1;
                            hackindex = 1;
                            index = displayHeadlines(1, stories, rss.Root.Element("channel").Element("title").Value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed! " + ex.Message);
                        }
                    }
                }

                if (input[0] == "o")
                {
                    int num;
                    if (int.TryParse(input[1], out num))
                    {
                        if (num > 0 && num < 26)
                        {
                            try
                            {
                                Match link = Regex.Match(stories[--num].Element("description").Value, "(?<=<a href=\")[^\"]*(?=\">\\[link])");
                                if (link.Captures.Count > 0)
                                    System.Diagnostics.Process.Start(link.Captures[0].Value);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                    
                }

                if (input[0] == "h")
                {
                    Console.Clear();

                    index = hackindex;
                    Console.WriteLine("rd Help");
                    Console.WriteLine("\nGeneral commands:\n");
                    Console.WriteLine("\tr subreddit\tLoads the provided subreddit");
                    Console.WriteLine("\tb\t\tBoss mode (Shows contents of C:\\Windows)");
                    Console.WriteLine("\tq or exit\tExits rd");
                    Console.WriteLine("\nWhile viewing stories:\n");
                    Console.WriteLine("\tn or <Enter>\tSee the next page of stories");
                    Console.WriteLine("\tp\t\tSee the preview page of stories");
                    Console.WriteLine("\t<number>\tShows the URL (or description for self.reddit)");
                    Console.WriteLine("\to <number>\tOpens the story in your default browser");
                    Console.WriteLine("\tc <number>\tView the comments");
                    Console.WriteLine("\nWhile viewing comments:\n");
                    Console.WriteLine("\tn or <Enter>\tSee the next page of comments");
                    Console.WriteLine("\ts\t\tSwitch back to the stories");
                }

                if (input[0] == "c")
                {
                    int num;
                    if (int.TryParse(input[1], out num))
                    {
                        if (num > 0 && num < 26)
                        {
                            Console.Write("Fetching comments... ");
                            comments = loadComments(stories[--num].Element("link").Value);
                            if (comments.Count > 1)
                            {
                                index_c = 1;
                                index_c = displayComments(index_c, comments);
                                viewingComments = true;
                            }
                            else
                            {
                                Console.WriteLine("failed. No comments found.");
                            }
                        }
                    }
                }

                if (input[0] == "b")
                {
                    Console.Title = @"C:\WINDOWS\System32\cmd.exe";
                    System.Diagnostics.Process boss = new System.Diagnostics.Process();
                    boss.StartInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");
                    boss.StartInfo.CreateNoWindow = true;
                    boss.StartInfo.RedirectStandardOutput = true;
                    boss.StartInfo.RedirectStandardInput = true;
                    boss.StartInfo.UseShellExecute = false;
                    boss.Start();
                    System.IO.StreamReader reader = new System.IO.StreamReader(boss.StandardOutput.BaseStream);
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(boss.StandardInput.BaseStream);
                    writer.WriteLine("C:");
                    writer.Flush();
                    writer.WriteLine("cd \\Windows");
                    writer.Flush();
                    writer.WriteLine("dir C:\\Windows");
                    writer.Close();
                    writer.Dispose();

                    Console.Write(reader.ReadToEnd());
                    reader.Close();
                    reader.Dispose();
                }

                else
                {
                    int num;
                    if (int.TryParse(input[0], out num))
                    {
                        if (num > 0 && num < 26)
                        {
                            index = hackindex;
                            displayDescription(--num, stories);
                        }
                    }
                }

                if (input[0] != "b")
                    Console.Write("\n> ");
                input = Console.ReadLine().Split(' ');
            }

            
        }

        static int displayHeadlines(int index, List<XElement> stories, string title)
        {
            if (index > 25)
                return 1;

            int rows = 0;
            int i = index;
            string[] story;

            // Clear the window
            Console.Clear();

            // Print the title
            Console.Title = title;
            Console.WriteLine("Now viewing: " + title + "\n");
            rows++; rows++;

            while (i <= stories.Count)
            {
                story = printHeadline(i, stories[i - 1]);

                if (rows + story.Length < Console.WindowHeight - 2)
                {
                    i++;
                    foreach (string line in story)
                    {
                        Console.WriteLine(line);
                        rows++;
                    }
                }
                else
                {
                    break;
                }
            }

            return i;
        }

        static int displayComments(int index, List<XElement> comments)
        {
            if (index >= comments.Count)
                return 1;

            int i = index;
            int rows = 0;
            string[] comment;

            Console.Clear();

            //Print the headline
            if (comments[0].Element("title").Value.Length > Console.WindowWidth - 17)
                Console.WriteLine("Comments on: " + comments[0].Element("title").Value.Substring(0,Console.WindowWidth - 17) + "...");
            else
                Console.WriteLine("Comments on: " + comments[0].Element("title").Value);
            rows++;

            while (i < comments.Count)
            {
                comment = printComment(i, comments[i]);

                if (rows + comment.Length < Console.WindowHeight - 2)
                {
                    i++;
                    foreach (string line in comment)
                    {
                        Console.WriteLine(line);
                        rows++;
                    }
                }
                else
                {
                    break;
                }
            }

            return i;
        }

        static void displayDescription(int num, List<XElement> stories)
        {


            string description = stories[num].Element("description").Value;
            if (description.Contains("<!-- SC_OFF -->"))
            {
                string title = html(stories[num].Element("title").Value);
                string[] splittitle = splitByLength(title, Console.WindowWidth - 1);

                Console.Clear();
                foreach (string line in splittitle)
                    Console.WriteLine(line);
                Console.WriteLine("");

                description = description.Substring(description.IndexOf("<!-- SC_OFF -->") + 15);
                description = description.Substring(0, description.IndexOf("<!-- SC_ON -->"));
                description = html(description);
                string[] split = splitByLength(description, Console.WindowWidth - 1);
                
                foreach (string line in split)
                    Console.WriteLine(line);
                Console.WriteLine("\tType \"c " + (num + 1).ToString() + "\" to view the comments.");
            }
            else
            {
                Match link = Regex.Match(description, "(?<=<a href=\")[^\"]*(?=\">\\[link])");
                if (link.Captures.Count > 0)
                    Console.WriteLine("Links to " + link.Captures[0].Value);
                Console.WriteLine("Type \"o " + (num + 1).ToString() + 
                    "\" to open this link in a browser, or \"c " + (num + 1).ToString() + "\" to view the comments.");
            }
        }

        static string[] splitByLength(string input, int length)
        {
            int k = 0;
            string[] words = input.Split(' ');
            List<string> output = new List<string>();
            string temp = "";

            while (k < words.Length)
            {
                if (temp.Length + words[k].Length + 1 > length)
                {
                    if (temp == "")
                    {
                        temp = words[k].Substring(0, length - 2) + "-";
                        words[k] = words[k].Substring(length - 2);
                    }
                    output.Add(temp);
                    temp = "";
                }
                else
                {
                    if (words[k] == "\n")
                    {
                        output.Add(temp);
                        temp = "";
                        k++;
                    }
                    else
                    {
                        if (words[k] != "")
                            temp += " " + words[k++];
                        else
                            k++;
                    }
                }
            }

            if (temp != "")
                output.Add(temp);

            return output.ToArray();
        }

        static void printInfo(string htmlin)
        {
            Regex submitter = new Regex("(?<=\"http://www.reddit.com/user/)[^\"]*(?=\")");
            Regex comments = new Regex(@"(?<=\[)\d{1,5}(?= comments])");

            if (submitter.IsMatch(htmlin))
            {
                Console.Write("\tSubmitted by " + submitter.Match(htmlin).Captures[0].Value);
            }

            if (comments.IsMatch(htmlin))
            {
                Console.Write("\t\t" + comments.Match(htmlin).Captures[0].Value + " comment(s)");
            }
            Console.Write("\n\n");
        }

        static string[] printHeadline(int index, XElement story)
        {
            List<string> output = new List<string>();
            string temp;

            // Write the index
            temp = index.ToString() + ".";

            // Print the story's title
            string[] split = splitByLength(html(story.Element("title").Value), Console.WindowWidth - 10);
            foreach (string line in split)
            {
                temp += ("\t" + line);
                output.Add(temp);
                temp = "";
            }

            // Print the submitter
            Match submitter = Regex.Match(story.Element("description").Value, "(?<=\"http://www.reddit.com/user/)[^\"]*(?=\")");
            if (submitter.Captures.Count > 0)
            {
                temp = new string(' ', 16);
                temp += "Submitted by " + submitter.Captures[0].Value;
            }

            // Print the number of comments
            Match comments = Regex.Match(story.Element("description").Value, @"(?<=\[)\d{1,5}(?= comments?])");
            if (comments.Captures.Count > 0)
            {
                if (temp.Length < 48)
                    temp += new string(' ', 48 - temp.Length);
                else
                    temp += "   ";
                
                temp += comments.Captures[0].Value + " comment";
                if (int.Parse(comments.Captures[0].Value) != 1)
                    temp += "s";
            }
            output.Add(temp);
            output.Add("");

            return output.ToArray();

        }

        static string[] printComment(int index, XElement comment)
        {
            List<string> output = new List<string>();
            string temp = "";


            Match username = Regex.Match(comment.Element("title").Value, "[^ ]*(?= )");
            if (username.Captures.Count > 0)
            {
                temp += username + " says:";
                output.Add(temp);
                temp = "";
            }


            string[] split = splitByLength(html(comment.Element("description").Value), Console.WindowWidth - 3);
            foreach (string line in split)
            {
                temp += ("  " + line);
                output.Add(temp);
                temp = "";
            }

            return output.ToArray();
        }

        static List<XElement> loadComments(string link)
        {
            string url = link + ".rss";
            List<XElement> ret = new List<XElement>();
            try
            {
                XDocument comments = XDocument.Load(url);
                ret = comments.Descendants("item").ToList<XElement>();
            }
            catch (Exception)
            {

            }
            return ret;
        }


        private static string html(string input)
        {
            string output = input;

            output = output.Replace("<div class=\"md\">", "");
            output = output.Replace("</div>", "");
            output = output.Replace("&quot;", "\"");
            output = output.Replace("&lt;", "<");
            output = output.Replace("&gt;", ">");
            output = output.Replace("&amp;", "&");
            output = output.Replace("<p>", "");
            output = output.Replace("</p>", " \n \n ");
            output = output.Replace("<blockquote>", "");
            output = output.Replace("</blockquote>", "");
            output = output.Replace("<strong>", "[");
            output = output.Replace("</strong>", "]");
            output = output.Replace("<a href=\"", "(").Replace("\" >", ")");
            output = output.Replace("</a>", "");

            return output;
        }

    }
}
