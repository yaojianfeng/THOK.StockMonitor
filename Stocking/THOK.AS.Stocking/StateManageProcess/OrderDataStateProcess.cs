using System;
using System.Collections.Generic;
using System.Text;

using THOK.MCP;
using THOK.Util;
using THOK.AS.Stocking.StateManageProcess.Dao;

namespace THOK.AS.Stocking.StateManageProcess
{
    class OrderDataStateProcess:AbstractProcess
    {
        /// <summary>
        /// ״̬�������б�
        /// </summary>
        private IDictionary<string, OrderDataStateManage> orderDataStateManages = new Dictionary<string, OrderDataStateManage>();
        private OrderDataStateManage GetStateManage(string stateItemCode)
        {
            if (!orderDataStateManages.ContainsKey(stateItemCode))
            {
                lock (orderDataStateManages)
                {
                    if (!orderDataStateManages.ContainsKey(stateItemCode))
                    {
                        orderDataStateManages[stateItemCode] = new OrderDataStateManage(stateItemCode,this.Context.ProcessDispatcher);
                    }
                }
            }
            return orderDataStateManages[stateItemCode];
        }

        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            /*
             * stateItem.Name �� ��Ϣ��Դ 
             * stateItem.ItemName ��
             *      ��Ӧ (0)Init                   �� ��ʼ��������������ʱ�ĳ�ʼ������
             *           (1)StateItemCode_OrderDataMoveNext �� ����PLC���ݵ�Ԫ������д������PLC���õ�ǰ����������ˮ�ţ�����д�������ݣ�
             *           (2)StateItemCode_OrderDataMoveTo   �� ����PLC���ݵ�Ԫ������������ݣ�PLC���õ�ǰ����������ˮ�ţ�����������ݣ�
             *           
             * stateItem.State  
             */
            try
            {
                using (PersistentManager pm = new PersistentManager())
                {
                    if (stateItem.ItemName == "Init")
                    {
                        foreach (OrderDataStateManage orderDataStateManageItem in orderDataStateManages.Values)
                        {
                            orderDataStateManageItem.MoveTo(1);
                        }
                        return;
                    }

                    string stateItemCode = stateItem.ItemName.Split('_')[0];
                    string action = stateItem.ItemName.Split('_')[1];
                    OrderDataStateManage orderDataStateManage = GetStateManage(stateItemCode);
                    int index = 0;

                    switch (action)
                    {
                        case "OrderDataMoveNext":
                            index = Convert.ToInt32(THOK.MCP.ObjectUtil.GetObject(stateItem.State));
                            if (index != 0 && orderDataStateManage.Check(index))
                            {
                                if (orderDataStateManage.WriteToPlc())
                                {
                                    Logger.Info(string.Format("��������д��ɹ���ˮ�ţ�[{0}]",index));
                                }
                            }
                            break;
                        case "OrderDataMoveTo":
                            index = Convert.ToInt32(THOK.MCP.ObjectUtil.GetObject(stateItem.State));
                            if (index != 0)
                            {
                                orderDataStateManage.MoveTo(index);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("OrderProcess.StateChanged() ����ʧ�ܣ�ԭ��" + e.Message);
            }
        }
    }
}