import pyxeds2

import time
print "hello"

lv=pyxeds2.createLive()
err=lv.init(pyxeds2.LiveMode.ReadWrite,'0.0.0.0',0,'10.7.224.2',43000,0,32767)
print pyxeds2.liveErrMessage(err)

arch=pyxeds2.createArch()
err=arch.init('0.0.0.0',0,'10.7.224.2',43001,0,50)
print pyxeds2.archErrMessage(err)



t=long(time.time())
print t


pt=lv.findByIESS('04VT_PS04DI-01.MCR@GRARM')
print pt
sid=lv.getSID(pt);
print sid

f=arch.getFunction('TOGGLE_UNDER')
f.pushPointParam(sid,0xff,pyxeds2.ArchShadeMode.PreferArch)
f.pushValueParam(0.1);
f.pushTimestampParam(t-3600*24*6)
f.pushTimestampParam(t-3600*24*5)
fID=arch.addQuery(f)
print fID

arch.executeQueries()
try:

    result=arch.getResponse(fID)
    print result
except:
    print 'error'

arch.clear()

f=arch.getFunction('TOGGLE_OVER')
f.pushPointParam(sid,0xff,pyxeds2.ArchShadeMode.PreferArch)
f.pushValueParam(0.9);
f.pushTimestampParam(t-3600*24*6)
f.pushTimestampParam(t-3600*24*5)
fID=arch.addQuery(f)
print fID
arch.executeQueries()
try:

    result=arch.getResponse(fID)
    print result
except:
    print 'error'

