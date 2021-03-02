export const server = 'https://localhost:44323';

export const webAPIUrl = `${server}/api`;

export const authSettings = {
  domain: 'dev-8sa0w9kc.eu.auth0.com',
  client_id: 'VpPA3r2kWOGZMzSTLbssjrdreUj8Bbuf',
  redirect_uri: window.location.origin + '/signin-callback',
  scope: 'openid profile QandAAPI email',
  audience: 'https://qanda',
};
