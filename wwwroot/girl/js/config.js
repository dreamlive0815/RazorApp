
var config = {
    apiUrlPrefix: '/girl/',
    pageUrlPrefix: './',
    defaultAutoSlideInterval: 5000,
    defaultHeadText: 'ZNGirls',
    defaultNavs: [

    ],

    getProfileApiUrl: function(girlId) {
        return this.apiUrlPrefix + 'profile/' + girlId;
    },
    getAlbumsApiUrl: function(girlId) {
        return this.apiUrlPrefix + 'albumlist/' + girlId;
    },
    getAlbumsPageUrl: function(girlId) {
        return this.pageUrlPrefix + 'albums.html?girlId=' + girlId;
    },
    getAlbumInfoPageUrl: function(albumId) {
        return this.pageUrlPrefix + 'albuminfo.html?albumId=' + albumId;
    }
};