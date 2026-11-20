from pathlib import Path
p=Path('Assets/Lighthouse_Authored/CoastWalkAudit.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('cam.transform.position=transform.position-dir*5.3f+Vector3.up*3;cam.transform.LookAt(transform.position+dir*2+Vector3.up*1.5f);','GetComponent<LighthouseWalker>().FollowCamera();');p.write_text(s,encoding='utf-8')
