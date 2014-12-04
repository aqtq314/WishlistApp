function urlFor(actionName, controllerName)
{
    var actionNameValid = /^[A-Za-z0-9_]+$/.test(actionName);
    var controllerNameValid = /^[A-Za-z0-9_]+$/.test(controllerName);
    if (!actionNameValid || !controllerNameValid)
        throw new Error("Invalid action or controller name.");

    return "/" + controllerName + "/" + actionName;
}

function getVisibilities()
{
    return [
        { name: "Private", value: 0 },
        { name: "Public", value: 1 },
        { name: "Friend", value: 2 }
    ];
}

function getVisibilityName(value)
{
    return ["Private", "Public", "Friend"][value];
}

var wishlistApp = angular.module("wishlistApp", []);

wishlistApp.controller("user-view-controller", function ($scope, $http)
{
    // $scope.userId is set in ng-init attribute
    $http.post(urlFor("GetUserInfo", "User"), { userId: $scope.userId })
        .success(function (data, status, headers, config)
        {
            $scope.Info = data;
            $scope.Info.CreationDate = new Date($scope.Info.CreationDate + " UTC").toLocaleString();
        });

    $http.post(urlFor("GetFriendshipStatus", "Friendship"), { userId: $scope.userId })
        .success(function (data, status, headers, config)
        {
            $scope.Friendship = data;

            $scope.befriend = function (befriend)
            {
                $http.post(urlFor("Befriend", "Friendship"), { userId: $scope.userId, befriend: befriend })
                    .success(function (data, status, headers, config)
                    {
                        if (data.Success)
                            $scope.Friendship = data;
                    });
            }
        });
});

wishlistApp.controller("roll-view-controller", function ($scope, $http)
{
    $scope.visibilities = getVisibilities();

    var getRolls = function ()
    {
        $http.post(urlFor("GetRollsFor", "Roll"), { userId: $scope.userId })
            .success(function (data, status, headers, config)
            {
                $scope.UserName = data.UserName;
                $scope.Rolls = data.Rolls;
                $scope.Rolls.forEach(function (roll)
                {
                    roll.Time = new Date(roll.TimeUtc + " UTC").toLocaleString();

                    roll.editor = {
                        controlVisible: false,
                        tempContent: roll.Content,
                        showControl: function ()
                        {
                            this.tempContent = roll.Content;
                            this.controlVisible = true;
                        },
                        cancelEdit: function ()
                        {
                            this.controlVisible = false;
                        },
                        submitEdit: function ()
                        {
                            var param = {
                                rollId: roll.RollId,
                                model: { Content: this.tempContent }
                            };
                            $http.post(urlFor("EditRoll", "Roll"), param)
                                .success(function (data, status, headers, config)
                                {
                                    roll.Content = roll.editor.tempContent;
                                    roll.editor.controlVisible = false;
                                });
                        },
                        switchVisibility: function ()
                        {
                            var tempVisibility = (roll.Visibility + 1) % 3;
                            var param = {
                                rollId: roll.RollId,
                                visibility: tempVisibility
                            };
                            $http.post(urlFor("ChangeRollVisibility", "Roll"), param)
                                .success(function (data, status, headers, config)
                                {
                                    roll.Visibility = tempVisibility;
                                    roll.VisibilityString = getVisibilityName(roll.Visibility);
                                });
                        },
                    };
                });

                $scope.Success = data.Success;
            });
    };

    getRolls();

    $scope.adder = {
        controlVisible: false,
        content: "",
        visibility: 0,
        showControl: function ()
        {
            this.controlVisible = true;
        },
        submit: function ()
        {
            var param = {
                model: { Content: this.content },
                visibility: this.visibility
            };
            $http.post(urlFor("AddRoll", "Roll"), param)
                .success(function (data, status, headers, config)
                {
                    this.content = "";
                    getRolls();
                });
        }
    };
});

wishlistApp.controller("wishlist-view-controller", function ($scope, $http)
{
    $scope.visibilities = getVisibilities();

    var getWishlists = function ()
    {
        $http.post(urlFor("GetWishlistsFor", "Wishlist"), { userId: $scope.userId })
            .success(function (data, status, headers, config)
            {
                $scope.UserName = data.UserName;
                $scope.Wishlists = data.Wishlists;
                $scope.Wishlists.forEach(function (wl)
                {
                    wl.Time = new Date(wl.TimeUtc + " UTC").toLocaleString();

                    wl.deleter = {
                        delete: function ()
                        {
                            $http.post(urlFor("RemoveWishlist", "Wishlist"), { wishlistId: wl.WishlistId })
                                .success(function (data, status, headers, config)
                                {
                                    getWishlists();
                                });
                        }
                    };

                    wl.editor = {
                        switchVisibility: function ()
                        {
                            var tempVisibility = (wl.Visibility + 1) % 3;
                            var param = {
                                wishlistId: wl.WishlistId,
                                visibility: tempVisibility
                            };
                            $http.post(urlFor("ChangeWishlistVisibility", "Wishlist"), param)
                                .success(function (data, status, headers, config)
                                {
                                    wl.Visibility = tempVisibility;
                                    wl.VisibilityString = getVisibilityName(wl.Visibility);
                                });
                        },
                    };
                });

                $scope.Success = data.Success;
            });
    };

    getWishlists();

    $scope.adder = {
        controlVisible: false,
        content: {
            Title: "",
            WishlistItems: []
        },
        visibility: 0,
        showControl: function ()
        {
            this.controlVisible = true;
        },
        add: function ()
        {
            this.content.WishlistItems.push({ Content: "" });
        },
        removeLast: function ()
        {
            this.content.WishlistItems.pop();
        },
        submit: function ()
        {
            var param = {
                model: this.content,
                visibility: this.visibility
            };
            $http.post(urlFor("AddWishlist", "Wishlist"), param)
                .success(function (data, status, headers, config)
                {
                    $scope.adder.content = {
                        Title: "",
                        WishlistItems: []
                    };
                    getWishlists();
                });
        }
    };
});

wishlistApp.controller("friendship-search-controller", function ($scope, $http)
{
    $scope.UserNameModel = "";

    $scope.searchUser = function ()
    {
        $scope.Success = undefined;

        $http.post(urlFor("SearchUser", "Friendship"), { userName: $scope.UserNameModel })
            .success(function (data, status, headers, config)
            {
                $scope.Query = $scope.UserNameModel;
                $scope.Users = data.Users;
                $scope.Users.forEach(function (user)
                {
                    user.CreationDate = new Date(user.CreationDate + " UTC").toLocaleString();
                });

                $scope.Success = data.Success;
            });
    };
});


