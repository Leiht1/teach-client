import urllib.request as u, urllib.parse as p, http.cookiejar, re,json
s=u.build_opener(u.HTTPCookieProcessor(http.cookiejar.CookieJar()))
a='https://quaternius.itch.io/stylized-nature-megakit'
h=s.open(a).read().decode(); token=re.search(r'name="csrf_token" value="([^"]+)',h)[1]
r=s.open(u.Request(a+'/download_url',data=p.urlencode({'csrf_token':token}).encode(),headers={'Referer':a})).read(); print(r.decode())
if b'url' in r:
 d=json.loads(r); h=s.open(d['url']).read();open('ArtSource/OriginalFiles/nature-download.html','wb').write(h)
