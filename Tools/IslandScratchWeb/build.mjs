import {build} from 'esbuild';
import fs from 'node:fs/promises';
import path from 'node:path';
const out=path.resolve('../../Assets/StreamingAssets/IslandSystem/Web');
await fs.mkdir(out,{recursive:true});
await build({entryPoints:['src/app.js'],bundle:true,format:'iife',outfile:path.join(out,'app.js'),minify:true,legalComments:'eof',target:'chrome120'});
for(const f of ['index.html','style.css'])await fs.copyFile('src/'+f,path.join(out,f));
await fs.cp('node_modules/blockly/media',path.join(out,'media'),{recursive:true});
await fs.cp('node_modules/scratch-blocks/media',path.join(out,'media'),{recursive:true});
await fs.mkdir(path.join(out,'licenses'),{recursive:true});
for(const pkg of ['scratch-blocks','blockly','@blockly/continuous-toolbox','@blockly/field-colour']){const root='node_modules/'+pkg;for(const f of ['LICENSE','LICENSE.md','LICENSE.txt','TRADEMARK','NOTICE'])try{await fs.copyFile(root+'/'+f,path.join(out,'licenses',pkg.replaceAll('/','-')+'-'+f));}catch(e){if(e.code!=='ENOENT')throw e;}}
await fs.copyFile('licenses/blockly-LICENSE',path.join(out,'licenses/blockly-LICENSE'));
const pkgs={};for(const pkg of ['scratch-blocks','blockly','@blockly/continuous-toolbox','@blockly/field-colour'])pkgs[pkg]=JSON.parse(await fs.readFile('node_modules/'+pkg+'/package.json','utf8')).version;
await fs.writeFile(path.join(out,'licenses/versions.json'),JSON.stringify(pkgs,null,2));
console.log('Bundled official Scratch Blocks',pkgs,'into',out);


