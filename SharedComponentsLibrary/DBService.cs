using Microsoft.EntityFrameworkCore;
using SharedComponentsLibrary.DTO;
using SharedComponentsLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TournamentsBracketsBase;

namespace SharedComponentsLibrary
{
    public class DBService
    {
        TournamentsContext dbContext;
        public DBService(string databaseName)
        {
            dbContext = new TournamentsContext(databaseName);
            dbContext.Database.EnsureCreated();
            if (dbContext.Competitors.Find(-1l) == null)
            {
                dbContext.Competitors.Add(new Competitor() { Id = -1, IsBye = 1, FirstName = "", LastName = "", Club = "" });
                dbContext.SaveChanges();
            }
        }

        public void UpdateTournament(Tournament tournament)
        {
            if (dbContext.Tournaments.Find(tournament.Id) == null)
                throw new ArgumentException("No such tournament");

            var tour = dbContext.Tournaments.Find(tournament.Id);
            tour.Name = tournament.Name;

            dbContext.SaveChanges();
        }

        public void UpdateCategory(CategoryDTO category)
        {
            if (!dbContext.Categories.Where(a => a.Id == category.Id).Any())
                throw new ArgumentException("No such category");

            var cat = dbContext.Categories.Where(a => a.Id == category.Id).First();
            cat.Tournament = category.Tournament;
            cat.Name = category.Name;
            cat.Type = category.Type;

            dbContext.SaveChanges();
        }

        public void UpdateCompetitor(CompetitorDTO competitor)
        {
            if (!dbContext.Competitors.Where(a => a.Id == competitor.Id).Any())
                throw new ArgumentException("No such competitor");

            var comp = dbContext.Competitors.Where(a => a.Id == competitor.Id).First();
            comp.FirstName = competitor.FirstName;
            comp.LastName = competitor.LastName;
            comp.Club = competitor.Club;
            comp.Status = competitor.Status;

            dbContext.SaveChanges();
        }

        public void UpdateRound(RoundDTO round, IList<IMatch> matches)
        {
            if(!dbContext.Rounds.Where(a => a.Id == round.Id).Any())
                throw new ArgumentException("No such round");

            foreach (var match in matches)
                UpdateMatch(round, match);
        }

        public void UpdateMatch(RoundDTO round, IMatch match)
        {
            if (!dbContext.Matches.Where(a => a.Id == match.ID && a.Round == round.Id && a.Category == round.Category).Any())
                throw new ArgumentException("No such match");

            string akaScore = "";
            if (match.AKA != null)
                akaScore = String.Join(' ', match.AKA.AllScores);
            string aoScore = "";
            if (match.AO != null)
                aoScore = String.Join(' ', match.AO.AllScores);
            int senshu = 0;
            if (match.AKA != null && match.AKA.Senshu)
                senshu = 1;
            else if (match.AO != null && match.AO.Senshu)
                senshu = 2;

            var m = dbContext.Matches.Where(a => a.Id == match.ID && a.Round == round.Id && a.Category == round.Category).First();

            m.Aka = match.AKA == null ? null : match.AKA.ID;
            m.Ao = match.AO == null ? null : match.AO.ID;
            m.Winner = match.Winner == null ? null : match.Winner.ID;
            m.Looser = match.Looser == null ? null : match.Looser.ID;
            m.IsFinished = match.isFinished ? 1 : 0;
            m.AkaScore = akaScore;
            m.AoScore = aoScore;
            m.AkaC1 = match.AKA == null ? 0 : match.AKA.Fouls_C1;
            m.AkaC2 = match.AKA == null ? 0 : match.AKA.Fouls_C2;
            m.AoC1 = match.AO == null ? 0 : match.AO.Fouls_C1;
            m.AoC2 = match.AO == null ? 0 : match.AO.Fouls_C2;
            m.Senshu = senshu;

            var akaInCategory = dbContext.CompetitorCategories.Where(a => a.Category == round.Category && a.Competitor == m.Aka).FirstOrDefault();
            if(akaInCategory != null)
                akaInCategory.CompetitorStatus = match.AKA.Status;

            var aoInCategory = dbContext.CompetitorCategories.Where(a => a.Category == round.Category && a.Competitor == m.Ao).FirstOrDefault();
            if(aoInCategory != null)
                aoInCategory.CompetitorStatus = match.AO.Status;

            dbContext.SaveChanges();
        }


        public void AddCompetitor(CompetitorDTO competitor)
        {
            if (dbContext.Competitors.Where(a => a.Id == competitor.Id).Any())
                throw new ArgumentException("Competitor with such ID already exists");

            dbContext.Competitors.Add(new Competitor()
            {
                //Id = competitor.Id,
                FirstName = competitor.FirstName,
                LastName = competitor.LastName,
                Club = competitor.Club,
                Status = competitor.Status,
                IsBye = competitor.IsBye
            });
            dbContext.SaveChanges();
        }

        public long GetLastCompetitorID()
        {
            if (!dbContext.Competitors.Any())
                return -1;
            else
                return dbContext.Competitors.Last().Id;
        }

        public void AddTournament(Tournament tournament)
        {
            if (dbContext.Tournaments.Find(tournament.Id) != null)
                throw new ArgumentException("Tournament with such ID already exists");

            dbContext.Tournaments.Add(tournament);
            dbContext.SaveChanges();
        }

        public void AddCategory(CategoryDTO category)
        {
            if (dbContext.Categories.Where(a => a.Id == category.Id).Any())
                throw new ArgumentException("Category with such ID already exists");

            dbContext.Add(new Category()
            {
                Name = category.Name,
                Tournament = category.Tournament,
                Type = category.Type
            });
            dbContext.SaveChanges();
        }

        public bool IsCategoryGenerated(CategoryDTO category) => dbContext.Rounds.Where(a => a.Category == category.Id).Any();

        public void AddCompetitorToCategory(CompetitorDTO competitor, CategoryDTO category)
        {
            if (dbContext.CompetitorCategories.Where(a => a.Competitor == competitor.Id && a.Category == category.Id).Any())
                throw new ArgumentException("This competitor is already in category");

            dbContext.CompetitorCategories.Add(new CompetitorCategory()
            {
                Category = category.Id,
                Competitor = competitor.Id,
                CompetitorStatus = competitor.Status == null ? 0 : (long)competitor.Status
            });
            dbContext.SaveChanges();
        }

        public void AddGeneratedCategory(CategoryDTO dto, ICategory category)
        {
            if (dbContext.Categories.Where(a => a.Id == dto.Id).Any())
            {
                var cat = dbContext.Categories.Where(a => a.Id == dto.Id).First();
                cat.Type = dto.Type;
                cat.Name = dto.Name;
            }
            else
                AddCategory(dto);

            dbContext.Winners.RemoveRange(dbContext.Winners.Where(a => a.Category == dto.Id));
            dbContext.Matches.RemoveRange(dbContext.Matches.Where(a => a.Category == dto.Id));
            dbContext.Rounds.RemoveRange(dbContext.Rounds.Where(a => a.Category == dto.Id));
            dbContext.SaveChanges();

            foreach (var round in category.Rounds)
            {
                var r = new Round()
                {
                    Id = round.ID,
                    Category = dto.Id,
                    Repechage = -1
                };
                dbContext.Rounds.Add(r);

                foreach (var match in round.Matches)
                    AddMatch(match, r);
            }

            dbContext.SaveChanges();
        }

        private void AddMatch(IMatch match, Round round)
        {
            string akaScore = "";
            if (match.AKA != null && match.AKA.AllScores?.Count > 0)
                akaScore = String.Join(' ', match.AKA.AllScores);
            string aoScore = "";
            if (match.AO != null && match.AO.AllScores?.Count > 0)
                aoScore = String.Join(' ', match.AO.AllScores);
            int senshu = 0;
            if (match.AKA != null && match.AKA.Senshu)
                senshu = 1;
            else if (match.AO != null && match.AO.Senshu)
                senshu = 2;

            dbContext.Matches.Add(new Match()
            {
                Id = match.ID,
                Round = round.Id,
                Category = round.Category,
                Aka = match.AKA == null ? null : match.AKA.ID,
                Ao = match.AO == null ? null : match.AO.ID,
                Winner = match.Winner == null ? null : match.Winner.ID,
                Looser = match.Looser == null ? null : match.Looser.ID,
                IsFinished = match.isFinished ? 1 : 0,
                AkaScore = akaScore,
                AoScore = aoScore,
                AkaC1 = match.AKA == null ? 0 : match.AKA.Fouls_C1,
                AkaC2 = match.AKA == null ? 0 : match.AKA.Fouls_C2,
                AoC1 = match.AO == null ? 0 : match.AO.Fouls_C1,
                AoC2 = match.AO == null ? 0 : match.AO.Fouls_C2,
                Senshu = senshu
            });
            dbContext.SaveChanges();
        }

        public void AddGeneratedRepechage(TournamentTree.Category category, CategoryDTO dto)
        {
            if (!dbContext.Categories.Where(a => a.Id == dto.Id).Any())
                throw new ArgumentException("No such category");

            if (dbContext.Rounds.Where(a => a.Category == dto.Id && a.Repechage != -1).Any())
            {
                var rounds = dbContext.Rounds.Where(a => a.Category == dto.Id && a.Repechage != -1).ToList();
                foreach (var r in rounds)
                {
                    dbContext.Matches.RemoveRange(dbContext.Matches.Where(a => a.Category == dto.Id && a.Round == r.Id));
                    dbContext.Rounds.Remove(r);
                }
                dbContext.SaveChanges();
            }

            if (category.RepechageAKA != null)
            {
                var r = new Round()
                {
                    Id = category.Rounds.Count,
                    Category = dto.Id,
                    Repechage = 0
                };
                dbContext.Rounds.Add(r);
                foreach (var match in category.RepechageAKA.Matches)
                    AddMatch(match, r);
            }

            if (category.RepechageAO != null)
            {
                var r = new Round()
                {
                    Id = category.Rounds.Count + 1,
                    Category = dto.Id,
                    Repechage = 1
                };
                dbContext.Rounds.Add(r);
                foreach (var match in category.RepechageAO.Matches)
                    AddMatch(match, r);
            }

            if (category.BronzeMatch != null)
            {
                if (category.RepechageAKA != null)
                {
                    var r = new Round()
                    {
                        Id = category.Rounds.Count,
                        Category = dto.Id,
                        Repechage = 2
                    };
                    dbContext.Rounds.Add(r);
                    AddMatch(category.BronzeMatch, r);
                }
            }

            dbContext.SaveChanges();
        }

        public void AddWinners(CategoryDTO category, IList<WinnerDTO> winners)
        {
            if (!dbContext.Categories.Where(a => a.Id == category.Id).Any())
                throw new ArgumentException("No such category");

            dbContext.Winners.RemoveRange(dbContext.Winners.Where(a => a.Category == category.Id));
            dbContext.Winners.AddRange(winners.Select(a => new Winner()
            {
                Category = category.Id,
                Competitor = a.Competitor,
                Place = a.Place
            }));
            dbContext.SaveChanges();
        }

        public void RemoveWinners(CategoryDTO category)
        {
            if (!dbContext.Categories.Where(a => a.Id == category.Id).Any())
                throw new ArgumentException("No such category");

            dbContext.Winners.RemoveRange(dbContext.Winners.Where(a => a.Category == category.Id));
            dbContext.SaveChanges();
        }

        public void RemoveCompetitorFromCategory(CompetitorDTO competitor, CategoryDTO category)
        {
            if (!dbContext.CompetitorCategories.Where(a => a.Competitor == competitor.Id && a.Category == category.Id).Any())
                throw new ArgumentException("No such competitor in category");

            dbContext.CompetitorCategories.Remove(dbContext.CompetitorCategories.Where(a => a.Competitor == competitor.Id && a.Category == category.Id).First());
            dbContext.SaveChanges();
        }

        public void RemoveCompetitor(CompetitorDTO competitor)
        {
            if (!dbContext.Competitors.Where(a => a.Id == competitor.Id).Any())
                throw new ArgumentException("No such competitor");

            if (dbContext.CompetitorCategories.Where(a => a.Competitor == competitor.Id).Any() ||
                dbContext.Matches.Where(a => a.Aka == competitor.Id || a.Ao == competitor.Id).Any())
                throw new ArgumentException("Competitor is participating in some categories. Can't remove him.");
            
            dbContext.Competitors.Remove(dbContext.Competitors.Find(competitor.Id));
            dbContext.SaveChanges();
        }

        public void RemoveTournament(Tournament tournament)
        {
            if (dbContext.Tournaments.Find(tournament.Id) == null)
                throw new ArgumentException("No such tournament");

            foreach (var category in dbContext.Categories.Where(a => a.Tournament == tournament.Id))
                RemoveCategory(new CategoryDTO { Id = category.Id, Tournament = category.Tournament, Type = category.Type, Name = category.Name });

            dbContext.Tournaments.Remove(tournament);
            dbContext.SaveChanges();
        }

        public void RemoveCategory(CategoryDTO category)
        {
            if (!dbContext.Categories.Where(a => a.Id == category.Id).Any())
                throw new ArgumentException("No such category");

            RemoveWinners(category);
            dbContext.CompetitorCategories.RemoveRange(dbContext.CompetitorCategories.Where(a => a.Category == category.Id));
            dbContext.Matches.RemoveRange(dbContext.Matches.Where(a => a.Category == category.Id));
            dbContext.Rounds.RemoveRange(dbContext.Rounds.Where(a => a.Category == category.Id));

            dbContext.Categories.Remove(dbContext.Categories.Where(a => a.Id == category.Id).First());
            dbContext.SaveChanges();
        }

        public CompetitorDTO GetCompetitor(long id)
        {
            if (!dbContext.Competitors.Where(a => a.Id == id).Any())
                throw new ArgumentException("No such competitor");

            var comp = dbContext.Competitors.Where(a => a.Id == id).First();

            return new CompetitorDTO()
            {
                Id = comp.Id,
                FirstName = comp.FirstName,
                LastName = comp.LastName,
                Club = comp.Club,
                Status = comp.Status,
                IsBye = comp.IsBye
            };
        }

        public void SwapCompetitors(CategoryDTO category, CompetitorDTO competitor1, CompetitorDTO competitor2)
        {
            if (dbContext.Matches.Where(a => a.Category == category.Id && a.IsFinished == 1 && a.Round > 0
            && (a.Aka == competitor1.Id || a.Aka == competitor2.Id ||
            a.Ao == competitor1.Id || a.Ao == competitor2.Id)).Any() || competitor1.Id == competitor2.Id)
                throw new InvalidOperationException("Can't swap competitors");

            var matches1Aka = dbContext.Matches.Where(a => a.Category == category.Id && a.Aka == competitor1.Id).ToList();
            var matches1Ao = dbContext.Matches.Where(a => a.Category == category.Id && a.Ao == competitor1.Id).ToList();
            var matches2Aka = dbContext.Matches.Where(a => a.Category == category.Id && a.Aka == competitor2.Id).ToList();
            var matches2Ao = dbContext.Matches.Where(a => a.Category == category.Id && a.Ao == competitor2.Id).ToList();
            
            foreach(var match in matches1Aka)
            {
                match.Aka = competitor2.Id;
                if (match.IsFinished == 1)
                    match.Winner = competitor2.Id;
            }
            foreach (var match in matches2Ao)
            {
                match.Ao = competitor1.Id;
                if (match.IsFinished == 1)
                    match.Winner = competitor1.Id;
            }

            foreach (var match in matches1Ao)
            {
                match.Ao = competitor2.Id;
                if (match.IsFinished == 1)
                    match.Winner = competitor2.Id;
            }
            foreach (var match in matches2Aka)
            {
                match.Aka = competitor1.Id;
                if (match.IsFinished == 1)
                    match.Winner = competitor1.Id;
            }

            dbContext.SaveChanges();
        }

        public List<CompetitorDTO> GetCompetitors()
        {
            return dbContext.Competitors.Select(a => new CompetitorDTO()
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Club = a.Club,
                Status = a.Status,
                IsBye = a.IsBye
            }).Where(a => a.Id > -1).ToList();
        }

        public List<Tournament> GetTournaments()
        {
            return dbContext.Tournaments.ToList();
        }

        public List<CategoryDTO> GetCategoriesInTournament(Tournament tournament)
        {
            if (tournament == null || !dbContext.Tournaments.Where(a => a.Id == tournament.Id).Any())
                return new List<CategoryDTO>();

            var categories = dbContext.Categories.Where(a => a.Tournament == tournament.Id)
                .Select(a => new CategoryDTO()
                {
                    Name = a.Name,
                    Id = a.Id,
                    Type = a.Type
                });

            return categories.ToList();
        }

        public List<CategoryDTO> GetGeneratedCategoriesInTournament(Tournament tournament)
        {
            return GetCategoriesInTournament(tournament).Where(a => IsCategoryGenerated(a)).ToList();
        }

        public List<CompetitorDTO> GetCompetitorsInCategory(CategoryDTO category)
        {
            if (category == null || !dbContext.Categories.Where(a => a.Id == category.Id).Any())
                return new List<CompetitorDTO>();

            var competitors = dbContext.CompetitorCategories.Where(a => a.Category == category.Id)
                .Select(a => new { a.Competitor, a.CompetitorStatus })
                .Join(dbContext.Competitors,
                a => a.Competitor,
                competitor => competitor.Id,
                (a, competitor) =>
                new CompetitorDTO()
                {
                    Id = competitor.Id,
                    FirstName = competitor.FirstName,
                    LastName = competitor.LastName,
                    Club = competitor.Club,
                    Status = a.CompetitorStatus,
                    IsBye = competitor.IsBye
                }).ToList();
            return competitors;
        }

        public List<RoundDTO> GetRoundsInCategory(CategoryDTO _category)
        {
            var category = dbContext.Categories.Find(_category.Id);
            if (category == null)
                throw new ArgumentException("No such category");

            var result = dbContext.Rounds.Include(a => a.Matches).Where(a => a.Category == category.Id)
                .Select(a => new RoundDTO
                {
                    Id = a.Id,
                    Category = a.Category,
                    Repechage = a.Repechage,
                    MatchesCount = a.Matches.Count
                }).ToList();

            return result;
        }

        public ICategory GetCategory(CategoryDTO _category)
        {
            ICategory result = null;

            var category = dbContext.Categories.Find(_category.Id);
            if (category == null)
                throw new KeyNotFoundException("No such category");

            var type = category.Type;
            //type: 0 - SE(2 third), 1 - SE(1 third), 3 - RR

            var competitors = GetCompetitorsInCategory(_category);

            var defaultRounds = dbContext.Rounds.Where(r => r.Category == _category.Id && r.Repechage == -1)
                .Include(round => round.Matches).ToList();

            if (type == 0 || type == 1)
                result = GetTournamentTreeCategory(competitors, _category.Id, (int)type, defaultRounds);
            else if (type == 2)
                result = GetRoundRobinCategory(competitors, _category.Id, defaultRounds);

            if (result != null)
                result.Winners = GetCategoryResults(_category);

            return result;
        }

        public List<ICompetitor> GetCategoryResults(CategoryDTO _category)
        {
            var winners = dbContext.Winners.Where(w => w.Category == _category.Id);
            var competitors = GetCompetitorsInCategory(_category);
            PriorityQueue<CompetitorDTO, long?> winnersList = new();
            if (winners != null)
                foreach (var winner in winners)
                    winnersList.Enqueue(competitors.Where(c => c.Id == winner.Competitor).FirstOrDefault(), winner.Place);

            var result = new List<ICompetitor>();
            while (winnersList.Count > 0)
            {
                var c = winnersList.Dequeue();
                result.Add(new TournamentTree.Competitor(false, (int)c.Id, c.FirstName, c.LastName, c.Club, 0, 0, 0, (int)c.Status));

            }

            return result;
        }

        RoundRobin.Category GetRoundRobinCategory(List<CompetitorDTO> competitors, long categoryId, List<Round>? defaultRounds)
        {
            var rrCategory = new RoundRobin.Category();
            rrCategory.Competitors = competitors
                .Select
                (c => (ICompetitor)new RoundRobin.Competitor(false, (int)c.Id, c.FirstName, c.LastName, c.Club, 0, 0, 0, (int)c.Status))
                .ToList();

            foreach (var round in defaultRounds)
            {
                RoundRobin.Round toAddRound = new RoundRobin.Round();
                toAddRound.ID = (int)round.Id;
                foreach (var match in round.Matches)
                {
                    var temp = GetTournamentTreeMatch(rrCategory.Competitors, match);
                    RoundRobin.Competitor aka = null;
                    if (temp.AKA != null)
                        aka = new RoundRobin.Competitor(temp.AKA as TournamentTree.Competitor);

                    RoundRobin.Competitor ao = null;
                    if (temp.AO != null)
                        ao = new RoundRobin.Competitor(temp.AO as TournamentTree.Competitor);
                    RoundRobin.Match toAddMatch = new RoundRobin.Match(aka, ao, (int)match.Id);

                    toAddMatch.isFinished = match.IsFinished == 1;
                    toAddMatch.Winner = temp.Winner;
                    toAddMatch.Looser = temp.Looser;

                    toAddRound.Matches.Add(toAddMatch);
                }
                rrCategory.Rounds.Add(toAddRound);
            }

            rrCategory.UpdateAllRounds();

            return rrCategory;
        }

        TournamentTree.Category GetTournamentTreeCategory(List<CompetitorDTO> competitors, long categoryId, int type, List<Round>? defaultRounds)
        {
            var akaRepechage = dbContext.Rounds.Where(r => r.Category == categoryId && r.Repechage == 0)
                .Include(round => round.Matches).FirstOrDefault();
            var aoRepechage = dbContext.Rounds.Where(r => r.Category == categoryId && r.Repechage == 1)
                .Include(round => round.Matches).FirstOrDefault();
            var bronzeMatch = dbContext.Rounds.Where(r => r.Category == categoryId && r.Repechage == 2)
                .Include(round => round.Matches).FirstOrDefault();

            var seCategory = new TournamentTree.Category();
            seCategory.Competitors = competitors.Select(c =>
                (ICompetitor)new TournamentTree.Competitor(false, (int)c.Id, c.FirstName, c.LastName, c.Club, 0, 0, 0, (int)c.Status)).ToList();

            foreach (var round in defaultRounds)
            {
                var toAddRound = new TournamentTree.Round();
                toAddRound.ID = (int)round.Id;
                foreach (var match in round.Matches)
                {
                    var toAddMatch = GetTournamentTreeMatch(seCategory.Competitors, match);
                    toAddRound.Matches.Add(toAddMatch);
                }
                toAddRound.Matches = toAddRound.Matches.OrderBy(a => a.ID).ToList();
                seCategory.Rounds.Add(toAddRound);
            }
            if (type == 0)
            {
                seCategory.is1third = false;
                seCategory.RepechageAKA = GetTournamentTreeRepechage(seCategory.Competitors, akaRepechage);
                seCategory.RepechageAO = GetTournamentTreeRepechage(seCategory.Competitors, aoRepechage);
            }
            else
            {
                seCategory.is1third = true;
                if (bronzeMatch != null)
                    seCategory.BronzeMatch = GetTournamentTreeMatch(seCategory.Competitors, bronzeMatch.Matches.FirstOrDefault());
            }

            return seCategory;
        }

        TournamentTree.Repechage GetTournamentTreeRepechage(List<ICompetitor> competitors, Round? repechage)
        {
            if (repechage == null)
                return null;
            TournamentTree.Repechage resultRepechage = new TournamentTree.Repechage();
            foreach (var match in repechage.Matches)
            {
                var toAddMatch = GetTournamentTreeMatch(competitors, match);

                if (toAddMatch.AKA != null && !resultRepechage.Competitors.Contains(toAddMatch.AKA))
                    resultRepechage.Competitors.Add(toAddMatch.AKA as TournamentTree.Competitor);
                if (toAddMatch.AO != null && !resultRepechage.Competitors.Contains(toAddMatch.AO))
                    resultRepechage.Competitors.Add(toAddMatch.AO as TournamentTree.Competitor);

                resultRepechage.Matches.Add(toAddMatch);
            }
            resultRepechage.Matches = resultRepechage.Matches.OrderBy(a => a.ID).ToList();
            if (resultRepechage.Matches.Count > 0 && resultRepechage.Matches[resultRepechage.Matches.Count - 1].Winner != null)
                resultRepechage.Winner = resultRepechage.Matches[resultRepechage.Matches.Count - 1].Winner as TournamentTree.Competitor;

            return resultRepechage;
        }

        TournamentTree.Match GetTournamentTreeMatch(List<ICompetitor> competitors, Match? match)
        {
            if (match == null)
                return null;

            TournamentTree.Competitor aka;
            if (match.Aka <= 0)
                aka = new TournamentTree.Competitor(true);
            else if (match.Aka != null)
                aka = new TournamentTree.Competitor(competitors.Where(c => c.ID == match.Aka).First() as TournamentTree.Competitor);
            else
                aka = null;
            if (aka != null)
            {
                aka.Senshu = match.Senshu == 1;
                aka.SetFoulsC1((int)match.AkaC1);
                foreach (var c in match.AkaScore)
                    if (Char.IsDigit(c))
                        aka.AddPoints(c - '0');
            }

            TournamentTree.Competitor ao;
            if (match.Ao <= 0)
                ao = new TournamentTree.Competitor(true);
            else if (match.Ao != null)
                ao = new TournamentTree.Competitor(competitors.Where(c => c.ID == match.Ao).First() as TournamentTree.Competitor);
            else
                ao = null;
            if (ao != null)
            {
                ao.Senshu = match.Senshu == 2;
                ao.SetFoulsC1((int)match.AoC1);
                foreach (var c in match.AoScore)
                    if (Char.IsDigit(c))
                        ao.AddPoints(c - '0');
            }

            var result = new TournamentTree.Match(aka, ao, (int)match.Id);
            
            result.isFinished = match.IsFinished == 1;
            if (match.Winner != null && match.Winner != 0)
                result.Winner = competitors.Where(c => c.ID == match.Winner).FirstOrDefault();
            if (match.Looser != null && match.Looser != 0)
                result.Looser = competitors.Where(c => c.ID == match.Looser).FirstOrDefault();

            return result;
        }
    }
}
