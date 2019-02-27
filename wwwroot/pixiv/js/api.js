/*
外部依赖
vue.js
vue-resource.js
util.js
auth.js
config.js
*/

var pixiv = {
    api: {
        getUserProfile(userId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                userId: userId,
            });
            Vue.http.post(config.getUserProfileApiUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        },
        followUser(userId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                userId: userId,
            });
            Vue.http.post(config.getFollowUserApiUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        },
        unFollowUser(userId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                userId: userId,
            });
            Vue.http.post(config.getUnFollowUserApiUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        },
        likeIllust(illustId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                illustId: illustId,
            });
            Vue.http.post(config.getLikeIllustUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        },
        bookmarkIllust(illustId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                illustId: illustId,
            });
            Vue.http.post(config.getBookmarkIllustApiUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        },
        unBookmarkIllust(bookId, okHandler, failHandler) {
            var params = mergeObj(getBasePostParams(), {
                bookId: bookId,
            });
            Vue.http.post(config.getUnBookmarkIllustApiUrl(), params, {emulateJSON:true}).then(okHandler, failHandler);
        }
    }
}