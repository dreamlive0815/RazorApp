
/*
依赖
config.js
*/

function getBasePostParams()
{
    return {
        cookies: getPixivCookies(),
    };
}

function getPixivCookies()
{
    return localStorage.getItem('pixivCookies');
}

function redirectToLoginPage()
{
    localStorage.setItem('referrer', window.location.href);
    window.location.href = config.getLoginPageUrl();
}

function redirectToLoginPageIfNoCookies()
{
    var cookies = getPixivCookies();
    if (!cookies) redirectToLoginPage();
}

function redirectToLoginPageIfCookiesExpired(res)
{
    if (!res.body.Message) return;
    if (res.body.Message.indexOf('请先登录') != -1) redirectToLoginPage();
}

function redirectToReferrerPage()
{
    var referrer = localStorage.getItem('referrer');
    if (referrer)
        window.location.href = referrer;
    else
        window.location.href = config.getIndexPageUrl();
}

function savePixivCookies(cookies)
{
    localStorage.setItem('pixivCookies', cookies);
}