#coding: utf-8

#date: 2015年9月29日01:50:37

#usage: edit steps and ledongli's uid(u need to download this app) .That would be ok .Good luck. ^_^

import csv

def main():

    # 导出csv

    # 根据csv制作字典
    entryList = []
    with open('tyme_for_cal.csv', 'r') as csvfile:
        spamreader = csv.reader(csvfile)
        keyrow = None
        for row in spamreader:
            if not keyrow:
                keyrow = row
                continue

            dict = {}
            for index in range(len(row)):
                dict[keyrow[index]] = row[index]
            entryList.append(dict)
        print entryList

    # 根据字典创建ical
    # 创建事件，填入时间
    from icalendar import Calendar, Event
    cal = Calendar()
    from datetime import datetime
    import pytz
    for entry in entryList:
        name = entry['Task']
        project = entry['Project']
        fullName = name + '#' + project
        date = entry['Date']
        startTime = entry['Start']
        endTime = entry['End']
        note = entry['Notes']
        print fullName + date + startTime + endTime
        startTime = convertTimeFormat(startTime)
        endTime = convertTimeFormat(endTime)
        date = date.split('/')
        for index in range(len(date)):
            date[index] = int(date[index])
        date[0] += 2000
        print fullName, date, startTime, endTime, note
        
        # 创建事件
        event = Event()
        event.add('summary', fullName)
        event.add('dtstart', datetime(date[0], date[1], date[2], startTime[0], startTime[1], 0))
        event.add('dtend', datetime(date[0], date[1], date[2], endTime[0], endTime[1], 0))
        event.add('DESCRIPTION', note)
        cal.add_component(event)

    # 写入ical
    import os
    f = open('tyme.ics', 'wb')
    f.write(cal.to_ical())
    f.close()

    # 发布ical到github

    return 0

def convertTimeFormat(timeStr):
    us = unicode(timeStr, "utf-8")
    tt = us[2:-1] + us[-1]
    tt = tt.split(':')
    for index in range(len(tt)):
        tt[index] = int(tt[index])
    if u'下午' == us[0:2]:
        tt[0] += 12
    return tt

if __name__ == '__main__':
    main()
