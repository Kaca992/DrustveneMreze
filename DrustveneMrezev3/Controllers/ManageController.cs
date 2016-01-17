using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DrustveneMrezev3.Managers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DrustveneMrezev3.Models;
using DrustveneMrezev3.MongoDB_objects;
using DrustveneMrezev3.MovieRecommendation;
using MongoDB.Bson;

namespace DrustveneMrezev3.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        #region passwords
        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
#endregion
        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        //
        //show user information

        
       
        public async Task<ActionResult> ShowUserInformation()
        {
            MongoDBManager mm = new MongoDBManager();
            UserInformation user = await mm.GetUserInformation(User.Identity.GetUserId());
            ShowUserInformationViewModel model = new ShowUserInformationViewModel()
            {
                Birthday = user.Birthday,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
            return View(model);
        }

        public async Task<ActionResult> ShowUserMovies()
        {
            MongoDBManager mm = new MongoDBManager();
            UserInformation user = await mm.GetUserInformation(User.Identity.GetUserId());
            List<MovieListModel> movies = new List<MovieListModel>();

            if (user.MovieLikes != null)
            {
                foreach (var userMovie in user.MovieLikes)
                {
                    MovieListModel newMovie = new MovieListModel()
                    {
                        AvgUserRating = userMovie.UserRating,
                        ID = userMovie.Id,
                        Title = userMovie.Name
                    };
                    var movie = await mm.GetMovie(userMovie.Id);
                    newMovie.Poster = movie.Poster;
                    movies.Add(newMovie);
                }
            }

            return View(movies);
        }

        public async Task<ActionResult> ShowMovieInformation(string id)
        {
            var oid = new ObjectId(id);
            MongoDBManager mm = new MongoDBManager();
            Movie movie = await mm.GetMovie(oid);
            var userRating = mm.GetUserRating(User.Identity.GetUserId(), oid);
            ShowMovieInformationViewModel model = new ShowMovieInformationViewModel()
            {
                ID = oid,
                Director = movie.Director,
                Actors = movie.Actors,
                Genres = movie.Genres,
                ImdbRating = movie.ImdbRating,
                TmdbRating = movie.TMDbRating,
                Title = movie.Title,
                Runtime = movie.Runtime,
                Poster = movie.Poster,
                Plot = movie.Plot,
                Language = movie.Language,
                MetascoreRating = movie.MetascoreRating,
                Released = movie.Released.Value,                
                AvgUserRating = movie.AvgUserRating                         
            };

            if (userRating != null)
            {
                model.Liked = true;
                model.UserRating = userRating.UserRating;
            }
            
            return View(model);
        }
        
        public async Task<ActionResult> UpdateUserRating(string movieID, int rating)
        {
            ObjectId oid = new ObjectId(movieID);
            MongoDBManager mm = new MongoDBManager();
            await mm.UpdateUserRating(User.Identity.GetUserId(), oid, rating);

            return Json(string.Empty);
        }

        public async Task<ActionResult> DislikeMovie(string movieID)
        {
            ObjectId oid = new ObjectId(movieID);
            MongoDBManager mm = new MongoDBManager();
            await mm.DislikeMovie(User.Identity.GetUserId(), oid);

            return Json(string.Empty);
        }

        public async Task<ActionResult> LikeMovie(string movieID)
        {
            ObjectId oid = new ObjectId(movieID);
            MongoDBManager mm = new MongoDBManager();
            await mm.LikeMovie(User.Identity.GetUserId(), oid);

            return Json(string.Empty);
        }

        public async Task<ActionResult> UpdateUserLikes(IndexViewModel model)
        {
            if (Session["tokenFB"] != null)
            {
                MongoDBManager mm = new MongoDBManager();
                List<MovieLike> movies = await mm.UpdateUserLikes(User.Identity.GetUserId(), Session["tokenFB"].ToString());
                return View("ShowUserMovies",movies);
            }
            else
            {
                return View("ShowUserMovies", new List<MovieLike>());
            }

        }

        public ActionResult UpdateTweets()
        {
            MongoDBManager mm = new MongoDBManager();
            mm.InsertNewTweets();

            return View("ShowTweets", mm.GetAllTweets());
        }

        public ActionResult ShowTweets()
        {
            MongoDBManager mm = new MongoDBManager();
            return View("ShowTweets", mm.GetAllTweets());
        }

        public async Task<ActionResult> MovieRecommendations(string recommendationValue = "")
        {
            List<MovieListModel> moviesModel = new List<MovieListModel>();
            IMovieRecomendationProvider recomendation;
            switch (recommendationValue)
            {
                case "1":
                    recomendation = new GenreMovieRecomendations();
                    break;
                case "2":
                    recomendation = new ActorMovieRecomendations();
                    break;
                case "3":
                    recomendation = new MoviesBasedOnPeople();
                    break;
                default:
                    return View(moviesModel);
            }

            List<Movie> movies = await recomendation.GetRecommendedMovies(User.Identity.GetUserId());

            foreach (var userMovie in movies)
            {
                MovieListModel newMovie = new MovieListModel()
                {
                    AvgUserRating = userMovie.AvgUserRating,
                    ID = userMovie.ID,
                    Title = userMovie.Title,
                    Poster = userMovie.Poster
                };

                moviesModel.Add(newMovie);
                
                
            }

            return View(moviesModel);

        }
        

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}