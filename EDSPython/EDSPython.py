import pyxeds2
import time
print "hello"
arch=pyxeds2.createArch()
err=arch.init('0.0.0.0',0,'10.7.224.2',43001,0,32768)
print pyxeds2.archErrMessage(err)

lv=pyxeds2.createLive()
err=arch.init('0.0.0.0',0,'10.7.224.2',43001,0,32768)
print pyxeds2.liveErrMessage(err)

t=long(time.time())
print t

f1=arch.getFunction('VALUE')
f1.pushPointParam(54435,0xff,pyxeds2.ArchShadeMode.PreferArch)
f1.pushTimestampParam(t-3600)
f1ID=arch.addQuery(f1)
print f1ID


f2=arch.getFunction('MAX_VALUE')
f2.pushPointParam(54435,0xff,pyxeds2.ArchShadeMode.PreferArch)
f2.pushTimestampParam(t-60)
f2.pushTimestampParam(t)
f2ID=arch.addQuery(f2)
print f2ID


arch.executeQueries()

try:
    result=arch.getResponse(f1ID)
    print result
    result=arch.getResponse(f2ID)
    print result
except:
    print 'error'

